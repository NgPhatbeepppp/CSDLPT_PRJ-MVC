using Microsoft.AspNetCore.Mvc;
using CSDLPT.Web.Models;
using System.Threading.Tasks;

public sealed class DoiBongController : Controller
{
    private readonly IDoiBongRepo _repo;
    public DoiBongController(IDoiBongRepo repo) => _repo = repo;

    // GET: /DoiBong
    public async Task<IActionResult> Index()
    {
        var items = await _repo.GetAllAsync();
        return View(items);
    }

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
            TempData["ok"] = "Tạo đội bóng thành công.";
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
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(clb)) return NotFound();
        var x = await _repo.FindAsync(id, clb);
        if (x is null) return NotFound();
        return View(x);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, string clb, DoiBong vm)
    {
        if (id != vm.MaDB || clb != vm.CLB) return BadRequest();
        if (!ModelState.IsValid) return View(vm);
        try
        {
            await _repo.UpdateAsync(vm);
            TempData["ok"] = "Cập nhật đội bóng thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
            return View(vm);
        }
    }

    // Delete tương tự
    public async Task<IActionResult> Delete(string id, string clb)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(clb)) return NotFound();
        var x = await _repo.FindAsync(id, clb);
        if (x is null) return NotFound();
        return View(x);
    }

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
            var x = await _repo.FindAsync(id, clb);
            return View("Delete", x);
        }
    }

}
