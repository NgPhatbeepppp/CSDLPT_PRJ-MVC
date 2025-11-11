using CSDLPT.Web.Models;
using CSDLPT.Web.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectList
using System.Collections.Generic; // Cần cho IEnumerable

namespace CSDLPT.Web.Controllers
{
    public sealed class CauThuController : Controller
    {
        private readonly ICauThuRepo _repo;
        private readonly IDoiBongRepo _doiBongRepo; // Inject thêm Repo Đội bóng

        // Constructor nhận cả 2 Repo
        public CauThuController(ICauThuRepo repo, IDoiBongRepo doiBongRepo)
        {
            _repo = repo;
            _doiBongRepo = doiBongRepo;
        }

        // --- HÀM INDEX (Tương tự DoiBongController) ---
        public async Task<IActionResult> Index(bool showGlobal = false)
        {
            IEnumerable<CauThu> cauThus;

            try
            {
                cauThus = await _repo.GetAllAsync(showGlobal);
            }
            catch (SqlException ex) when (showGlobal && IsNetworkError(ex))
            {
                TempData["ErrorMessage"] = $"Không thể tải dữ liệu toàn cục. Lỗi kết nối: {ex.Message}. Đang hiển thị dữ liệu cục bộ.";
                cauThus = await _repo.GetAllAsync(isGlobal: false);
                showGlobal = false;
            }

            ViewData["IsGlobalView"] = showGlobal;
            return View(cauThus);
        }

        private bool IsNetworkError(SqlException ex)
        {
            // Lỗi 7391: Lỗi khi thực thi RPC qua Linked Server (thường do 1 node offline)
            return ex.Number == 53 || ex.Number == -2 || ex.Number == 7391;
        }

        // --- HÀM DETAILS ---
        // GET: /CauThu/Details/A_CT01
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var x = await _repo.GetByIdAsync(id); // Luôn gọi GetById qua Coordinator
            if (x is null) return NotFound();
            return View(x);
        }

        // --- HÀM CREATE ---
        // GET: /CauThu/Create
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách Đội bóng TOÀN CỤC để làm Dropdown
            await PopulateDoiBongDropDownList();
            return View();
        }

        // POST: /CauThu/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CauThu vm)
        {
            // Kiểm tra validate của Model (bao gồm cả logic IValidatableObject)
            if (!ModelState.IsValid)
            {
                await PopulateDoiBongDropDownList(vm.MaDB);
                return View(vm);
            }
            try
            {
                await _repo.CreateAsync(vm); // Ghi qua Coordinator -> View v_CAUTHU -> Trigger -> RPC
                TempData["SuccessMessage"] = "Tạo cầu thủ thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi tạo cầu thủ: " + ex.Message);
                await PopulateDoiBongDropDownList(vm.MaDB);
                return View(vm);
            }
        }

        // --- HÀM EDIT ---
        // GET: /CauThu/Edit/A_CT01
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var x = await _repo.GetByIdAsync(id); // Luôn gọi GetById qua Coordinator
            if (x is null) return NotFound();

            await PopulateDoiBongDropDownList(x.MaDB);
            return View(x);
        }

        // POST: /CauThu/Edit/A_CT01
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CauThu vm)
        {
            if (id != vm.MaCT) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateDoiBongDropDownList(vm.MaDB);
                return View(vm);
            }
            try
            {
                await _repo.UpdateAsync(vm); // Ghi qua Coordinator -> View v_CAUTHU -> Trigger -> RPC
                TempData["SuccessMessage"] = "Cập nhật cầu thủ thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
                await PopulateDoiBongDropDownList(vm.MaDB);
                return View(vm);
            }
        }

        // --- HÀM DELETE ---
        // GET: /CauThu/Delete/A_CT01
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var x = await _repo.GetByIdAsync(id); // Luôn gọi GetById qua Coordinator
            if (x is null) return NotFound();
            return View(x);
        }

        // POST: /CauThu/Delete/A_CT01
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id) // Bỏ các tham số không dùng
        {
            try
            {
                await _repo.DeleteAsync(id); // Ghi qua Coordinator -> View v_CAUTHU -> Trigger -> RPC
                TempData["SuccessMessage"] = "Đã xóa cầu thủ."; // Đổi tên TempData cho nhất quán
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Nếu xóa lỗi (ví dụ: dính khóa ngoại ở bảng THAMGIA)
                TempData["ErrorMessage"] = "Không thể xóa: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }


        // --- HELPER: Lấy danh sách Đội Bóng cho Dropdown ---
        private async Task PopulateDoiBongDropDownList(object selectedDoiBong = null)
        {
            try
            {
                // QUAN TRỌNG: Luôn lấy danh sách TOÀN CỤC (isGlobal: true)
                // để đảm bảo Node C có thể thấy đội của Node A/B
                var doiBongs = await _doiBongRepo.GetAllAsync(isGlobal: true);

                ViewBag.DoiBongList = new SelectList(doiBongs, "MaDB", "TenDB", selectedDoiBong);
            }
            catch (SqlException ex) when (IsNetworkError(ex))
            {
                // Nếu không tải được list toàn cục (do mất mạng)
                ViewBag.DoiBongList = new SelectList(new List<DoiBong>(), "MaDB", "TenDB");
                ModelState.AddModelError("", $"Không thể tải danh sách đội bóng toàn cục. Lỗi: {ex.Message}");
            }
            catch (Exception ex)
            {
                ViewBag.DoiBongList = new SelectList(new List<DoiBong>(), "MaDB", "TenDB");
                ModelState.AddModelError("", $"Lỗi tải danh sách đội bóng: {ex.Message}");
            }
        }
    }
}