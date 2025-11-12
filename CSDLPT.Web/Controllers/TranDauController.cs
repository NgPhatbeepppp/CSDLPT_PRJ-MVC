// file: Controllers/TranDauController.cs
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

        public TranDauController(ITranDauRepo tranDauRepo, ISanRepo sanRepo)
        {
            _tranDauRepo = tranDauRepo;
            _sanRepo = sanRepo;
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

        // GET: TranDau/Details/TD01
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tranDau = await _tranDauRepo.GetByIdAsync(id);
            if (tranDau == null)
            {
                return NotFound();
            }

            return View(tranDau);
        }

        // GET: TranDau/Create
        public async Task<IActionResult> Create()
        {
            await LoadSanDropdownAsync();
            // Có thể truyền ViewData/ViewBag cho Dropdownlist MaSan nếu cần
            return View();
        }

        // POST: TranDau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NgayThiDau,MaSan,LuotDau,VongDau")] TranDau tranDau)
        {
            // Lưu ý: Không Bind MaTD vì MaTD được CSDL tự sinh
            //
            if (ModelState.IsValid)
            {
                try
                {
                    await _tranDauRepo.CreateAsync(tranDau);
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlException ex)
                {
                    // Xử lý lỗi (ví dụ: lỗi từ trigger,...)
                    ModelState.AddModelError("", $"Lỗi CSDL: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                }
            }
            await LoadSanDropdownAsync(tranDau.MaSan);
            return View(tranDau);
        }

        // GET: TranDau/Edit/TD01
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tranDau = await _tranDauRepo.GetByIdAsync(id);
            if (tranDau == null)
            {
                return NotFound();
            }
            await LoadSanDropdownAsync(tranDau.MaSan);
            // Có thể truyền ViewData/ViewBag cho Dropdownlist MaSan nếu cần
            return View(tranDau);
        }

        // POST: TranDau/Edit/TD01
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaTD,NgayThiDau,MaSan,LuotDau,VongDau")] TranDau tranDau)
        {
            if (id != tranDau.MaTD)
            {
                return NotFound();
            }

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
            return View(tranDau);
        }

        // GET: TranDau/Delete/TD01
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tranDau = await _tranDauRepo.GetByIdAsync(id);
            if (tranDau == null)
            {
                return NotFound();
            }

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
            catch (SqlException ex)
            {
                // Xử lý lỗi (ví dụ: lỗi FK từ bảng THAMGIA)
                ModelState.AddModelError("", $"Lỗi CSDL khi xóa: {ex.Message}");
                // Lấy lại đối tượng để hiển thị lại trang Delete
                var tranDau = await _tranDauRepo.GetByIdAsync(id);
                if (tranDau == null) return NotFound(); // Trường hợp hiếm gặp
                return View(tranDau);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                var tranDau = await _tranDauRepo.GetByIdAsync(id);
                if (tranDau == null) return NotFound();
                return View(tranDau);
            }
        }
    }
}