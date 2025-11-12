/* Nội dung file: CSDLPT.Web/Controllers/TranDauController.cs 
Mục đích: Sửa lỗi Create POST (Không Bind MaTD) và hoàn thiện các hàm khác
*/
using CSDLPT.Web.Models;
using CSDLPT.Web.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSDLPT.Web.Controllers
{
    public class TranDauController : Controller
    {
        private readonly ITranDauRepo _tranDauRepo;
        private readonly ISanRepo _sanRepo;
        private readonly IDoiBongRepo _doiBongRepo;
        private readonly ICauThuRepo _cauThuRepo;
        private readonly IThamGiaRepo _thamGiaRepo;

        public TranDauController(ITranDauRepo tranDauRepo, ISanRepo sanRepo,
                                 IDoiBongRepo doiBongRepo, ICauThuRepo cauThuRepo, IThamGiaRepo thamGiaRepo)
        {
            _tranDauRepo = tranDauRepo;
            _sanRepo = sanRepo;
            _doiBongRepo = doiBongRepo;
            _cauThuRepo = cauThuRepo;
            _thamGiaRepo = thamGiaRepo;
        }

        // (Các hàm Helper: PopulateDoiBongDropDownList, LoadSanDropdownAsync... giữ nguyên)
        private async Task PopulateDoiBongDropDownList(object? selectedDoiBong = null)
        {
            try
            {
                var doiBongs = await _doiBongRepo.GetAllAsync(isGlobal: true);
                ViewBag.DoiBongList = new SelectList(doiBongs, "MaDB", "TenDB", selectedDoiBong);
            }
            catch (Exception ex)
            {
                ViewBag.DoiBongList = new SelectList(new List<DoiBong>(), "MaDB", "TenDB");
                ModelState.AddModelError("", $"Lỗi tải danh sách đội bóng: {ex.Message}");
            }
        }
        private async Task LoadSanDropdownAsync(object? selectedSan = null)
        {
            var sanList = await _sanRepo.GetAllForDropdownAsync();
            ViewBag.MaSanList = new SelectList(sanList, "MaSan", "TenSan", selectedSan);
        }

        // GET: TranDau
        public async Task<IActionResult> Index()
        {
            var tranDaus = await _tranDauRepo.GetAllAsync();
            return View(tranDaus);
        }

        // GET: TranDau/Create
        public async Task<IActionResult> Create()
        {
            await LoadSanDropdownAsync();
            await PopulateDoiBongDropDownList();
            return View();
        }

        // POST: TranDau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // === PHIÊN BẢN CHÍNH XÁC (Sửa lỗi) ===
        // Không Bind MaTD
        public async Task<IActionResult> Create([Bind("NgayThiDau,MaSan,LuotDau,VongDau,MaDoiNha,MaDoiKhach")] TranDau tranDau)
        {
            // Bật lại
            if (ModelState.IsValid)
            {
                try
                {
                    // Dùng Repo đã sửa lỗi (ở trên)
                    await _tranDauRepo.CreateAsync(tranDau);
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError("", $"Lỗi CSDL: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                }
            }
            // Nếu lỗi, load lại dropdowns
            await LoadSanDropdownAsync(tranDau.MaSan);
            await PopulateDoiBongDropDownList();
            return View(tranDau);
        }

        // GET: TranDau/Edit/TD01
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var tranDau = await _tranDauRepo.GetByIdAsync(id);
            if (tranDau == null) return NotFound();

            await LoadSanDropdownAsync(tranDau.MaSan);

            var doiBongs = await _doiBongRepo.GetAllAsync(isGlobal: true);
            ViewBag.DoiNhaList = new SelectList(doiBongs, "MaDB", "TenDB", tranDau.MaDoiNha);
            ViewBag.DoiKhachList = new SelectList(doiBongs, "MaDB", "TenDB", tranDau.MaDoiKhach);

            return View(tranDau);
        }

        // POST: TranDau/Edit/TD01
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind MaTD là ĐÚNG vì đây là Update
        public async Task<IActionResult> Edit(string id, [Bind("MaTD,NgayThiDau,MaSan,LuotDau,VongDau,MaDoiNha,MaDoiKhach")] TranDau tranDau)
        {
            if (id != tranDau.MaTD) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _tranDauRepo.UpdateAsync(tranDau);
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError("", $"Lỗi CSDL khi cập nhật: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                }
            }
            await LoadSanDropdownAsync(tranDau.MaSan);
            var doiBongs = await _doiBongRepo.GetAllAsync(isGlobal: true);
            ViewBag.DoiNhaList = new SelectList(doiBongs, "MaDB", "TenDB", tranDau.MaDoiNha);
            ViewBag.DoiKhachList = new SelectList(doiBongs, "MaDB", "TenDB", tranDau.MaDoiKhach);
            return View(tranDau);
        }

        // GET: TranDau/Delete/TD01
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var tranDau = await _tranDauRepo.GetByIdAsync(id);
            if (tranDau == null) return NotFound();
            return View(tranDau);
        }

        // POST: TranDau/Delete/TD01
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _tranDauRepo.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}. (Có thể do còn Cầu thủ tham gia?)";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        // GET: TranDau/Details/TD01
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var tranDau = await _tranDauRepo.GetByIdAsync(id);
            if (tranDau == null) return NotFound();

            var dsThamGia = await _thamGiaRepo.GetByMaTDAsync(id);
            var allCauThu = await _cauThuRepo.GetAllAsync(isGlobal: true);

            var viewModel = new TranDauDetailsViewModel
            {
                TranDau = tranDau,
                DanhSachThamGia = dsThamGia,
                TenCauThuLookup = allCauThu.ToDictionary(ct => ct.MaCT, ct => $"{ct.Ho} {ct.Ten}"),

                CauThuOptions = allCauThu
                                .Where(ct => ct.MaDB == tranDau.MaDoiNha || ct.MaDB == tranDau.MaDoiKhach)
                                .Select(ct => new SelectListItem
                                {
                                    Value = ct.MaCT,
                                    Text = $"{ct.Ho} {ct.Ten} ({ct.MaCT})"
                                }).ToList(),

                NewThamGia = new ThamGia { MaTD = id, SoTrai = 0 }
            };

            return View(viewModel);
        }

        // === CÁC ACTION MỚI CHO THAMGIA ===

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddThamGia(TranDauDetailsViewModel viewModel)
        {
            var thamGiaMoi = viewModel.NewThamGia;
            try
            {
                await _thamGiaRepo.CreateAsync(thamGiaMoi);
                TempData["SuccessMessage"] = "Thêm cầu thủ thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thêm: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id = thamGiaMoi.MaTD });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteThamGia(string MaTD, string MaCT)
        {
            try
            {
                await _thamGiaRepo.DeleteAsync(MaTD, MaCT);
                TempData["SuccessMessage"] = "Xóa cầu thủ thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id = MaTD });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateThamGiaSoTrai(string MaTD, string MaCT, int SoTrai)
        {
            try
            {
                var thamGia = new ThamGia { MaTD = MaTD, MaCT = MaCT, SoTrai = SoTrai };
                await _thamGiaRepo.UpdateAsync(thamGia);
                TempData["SuccessMessage"] = "Cập nhật 'Số trái' thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id = MaTD });
        }
    }
}