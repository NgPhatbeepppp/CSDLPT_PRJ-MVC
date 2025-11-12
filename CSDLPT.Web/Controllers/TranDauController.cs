/* Nội dung file: CSDLPT.Web/Controllers/TranDauController.cs 
Mục đích: Sửa lỗi Create POST (Không Bind MaTD) và hoàn thiện các hàm khác
PHIÊN BẢN ĐÃ SỬA LỖI (Biên dịch và Hiệu năng)
*/
using CSDLPT.Web.Models;
using CSDLPT.Web.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq; // Cần thêm using này cho .Concat() và .ToDictionary()

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
            await PopulateDoiBongDropDownList(); // Đã chuẩn bị 'ViewBag.DoiBongList' cho cả 2 dropdown
            return View();
        }

        // POST: TranDau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind MaDoiNha, MaDoiKhach là chính xác
        public async Task<IActionResult> Create([Bind("NgayThiDau,MaSan,LuotDau,VongDau,MaDoiNha,MaDoiKhach")] TranDau tranDau)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Repo đã được viết để KHÔNG gửi MaTD (CSDL tự sinh)
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
            await PopulateDoiBongDropDownList(); // Tải lại danh sách đội bóng
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

            var dsThamGia_Full = await _thamGiaRepo.GetByMaTDAsync(id);
            var daThamGiaIds = dsThamGia_Full.Select(tg => tg.MaCT).ToHashSet();

            var cauThuDoiNha_Full = await _cauThuRepo.GetByDoiBongAsync(tranDau.MaDoiNha, isGlobal: true);
            var cauThuDoiKhach_Full = await _cauThuRepo.GetByDoiBongAsync(tranDau.MaDoiKhach, isGlobal: true);
            var allCauThu = await _cauThuRepo.GetAllAsync(isGlobal: true);
            var cauThuHaiDoi = cauThuDoiNha_Full.Concat(cauThuDoiKhach_Full).ToList();

            // SỬA 1: Tải TẤT CẢ đội bóng (để tra cứu tên)
            var allDoiBong = await _doiBongRepo.GetAllAsync(isGlobal: true);

            var maDoiBongLookup = allCauThu.ToDictionary(ct => ct.MaCT, ct => ct.MaDB ?? "KHAC");

            var viewModel = new TranDauDetailsViewModel
            {
                TranDau = tranDau,
                TenCauThuLookup = allCauThu.ToDictionary(ct => ct.MaCT, ct => ct.HoTen),
                MaDoiBongLookup = maDoiBongLookup,

                // SỬA 2: Gán bản đồ tra cứu Tên Đội Bóng
                TenDoiBongLookup = allDoiBong.ToDictionary(db => db.MaDB, db => db.TenDB),

                // (Các logic cũ cho Dropdown và Checkbox giữ nguyên...)
                CauThuOptions = new SelectList(
                    cauThuHaiDoi.Where(ct => !daThamGiaIds.Contains(ct.MaCT))
                                 .Select(ct => new { ct.MaCT, TenHienThi = $"{ct.HoTen} ({ct.MaCT})" }),
                    "MaCT", "TenHienThi"
                ),
                NewThamGia = new ThamGia { MaTD = id, SoTrai = 0 },
                CauThuDoiNha_ChuaThamGia = cauThuDoiNha_Full
                    .Where(ct => !daThamGiaIds.Contains(ct.MaCT))
                    .OrderBy(ct => ct.HoTen)
                    .ToList(),
                CauThuDoiKhach_ChuaThamGia = cauThuDoiKhach_Full
                    .Where(ct => !daThamGiaIds.Contains(ct.MaCT))
                    .OrderBy(ct => ct.HoTen)
                    .ToList(),
            };

            // Phân loại dsThamGia_Full vào 3 list (Đã có)
            foreach (var thamGia in dsThamGia_Full)
            {
                maDoiBongLookup.TryGetValue(thamGia.MaCT, out var maDB);

                if (maDB == tranDau.MaDoiNha)
                    viewModel.DanhSachThamGia_DoiNha.Add(thamGia);
                else if (maDB == tranDau.MaDoiKhach)
                    viewModel.DanhSachThamGia_DoiKhach.Add(thamGia);
                else
                    viewModel.DanhSachThamGia_Khac.Add(thamGia);
            }

            // SỬA 3: Tính tổng số trái cho mỗi đội (ngay trước khi return)
            viewModel.TongSoTrai_DoiNha = viewModel.DanhSachThamGia_DoiNha.Sum(tg => tg.SoTrai ?? 0);
            viewModel.TongSoTrai_DoiKhach = viewModel.DanhSachThamGia_DoiKhach.Sum(tg => tg.SoTrai ?? 0);

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchAddThamGia(string MaTD, List<string> SelectedCauThuIds)
        {
            if (SelectedCauThuIds == null || !SelectedCauThuIds.Any())
            {
                TempData["ErrorMessage"] = "Bạn chưa chọn cầu thủ nào.";
                return RedirectToAction(nameof(Details), new { id = MaTD });
            }

            int successCount = 0;
            try
            {
                // Lặp qua danh sách ID được chọn
                foreach (var maCT in SelectedCauThuIds)
                {
                    var thamGiaMoi = new ThamGia
                    {
                        MaTD = MaTD,
                        MaCT = maCT,
                        SoTrai = 0 // Mặc định là 0
                    };

                    // Gọi Repo cho từng cầu thủ
                    // Trigger tg_v_THAMGIA_INS sẽ xử lý định tuyến
                    await _thamGiaRepo.CreateAsync(thamGiaMoi);
                    successCount++;
                }

                TempData["SuccessMessage"] = $"Đã thêm thành công {successCount} cầu thủ.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thêm hàng loạt: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = MaTD });
        }
    }
}