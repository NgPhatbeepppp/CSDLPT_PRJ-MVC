using CSDLPT.Web.Models;
using CSDLPT.Web.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

public sealed class DoiBongController : Controller
{
    private readonly IDoiBongRepo _repo;
    public DoiBongController(IDoiBongRepo repo) => _repo = repo;

    // --- HÀM INDEX (ĐÃ CHUẨN - Giữ nguyên) ---
    public async Task<IActionResult> Index(bool showGlobal = false)
    {
        IEnumerable<DoiBong> doiBongs;

        try
        {
            doiBongs = await _repo.GetAllAsync(showGlobal);
        }
        catch (SqlException ex) when (showGlobal && IsNetworkError(ex))
        {
            TempData["ErrorMessage"] = $"Không thể tải dữ liệu toàn cục. Lỗi kết nối: {ex.Message}. Đang hiển thị dữ liệu cục bộ.";
            doiBongs = await _repo.GetAllAsync(isGlobal: false);
            showGlobal = false;
        }

        ViewData["IsGlobalView"] = showGlobal;
        return View(doiBongs);
    }

    private bool IsNetworkError(SqlException ex)
    {
        return ex.Number == 53 || ex.Number == -2 || ex.Number == 7391;
    }

    // --- HÀM CREATE (ĐÃ CHUẨN - Giữ nguyên) ---
    // GET: /DoiBong/Create
    public IActionResult Create() => View();

    // POST: /DoiBong/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoiBong vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            await _repo.CreateAsync(vm); // Write → Coord → Trigger RPC → SP mảnh
            TempData["SuccessMessage"] = "Tạo đội bóng thành công."; // Đổi tên TempData
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi tạo đội bóng: " + ex.Message);
            return View(vm);
        }
    }

    // GET: /DoiBong/Edit?id=TSST&clb=CLB1
    public async Task<IActionResult> Edit(string id, string clb) 
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var x = await _repo.GetByIdAsync(id); 
        if (x is null) return NotFound();
        return View(x);
    }

    // POST: /DoiBong/Edit/A_DB01
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, DoiBong vm) // Bỏ 'clb'
    {
        // Chỉ kiểm tra 'id' với 'vm.MaDB'
        if (id != vm.MaDB) return BadRequest();

        if (!ModelState.IsValid) return View(vm);
        try
        {
            await _repo.UpdateAsync(vm);
            TempData["SuccessMessage"] = "Cập nhật đội bóng thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
            return View(vm);
        }
    }

    // --- CẬP NHẬT HÀM DELETE ---
    // GET: /DoiBong/Delete/A_DB01
    // Delete tương tự
    public async Task<IActionResult> Delete(string id, string clb)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
       
        var x = await _repo.GetByIdAsync(id);
        if (x is null) return NotFound();
        return View(x);
    }
    // POST: /DoiBong/Delete/A_DB01
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id, string clb) 
    {
        try
        {
            await _repo.DeleteAsync(id);
            TempData["ok"] = "Đã xóa đội bóng.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Không thể xóa: " + ex.Message);
            var x = await _repo.GetByIdAsync(id);
            return View("Delete", x);
        }
    }

    // (Hàm Details nếu có - Tương tự Edit/Delete)
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var x = await _repo.GetByIdAsync(id);
        if (x is null) return NotFound();
        return View(x);
    }
}