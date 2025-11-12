using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace CSDLPT.Web.Models
{
    public class TranDauDetailsViewModel
    {
        public TranDau TranDau { get; set; }

        
        public IEnumerable<ThamGia> DanhSachThamGia { get; set; } // Sẽ bị thay thế logic
        public Dictionary<string, string> TenCauThuLookup { get; set; }
        public SelectList CauThuOptions { get; set; }
        public ThamGia NewThamGia { get; set; }
        public List<CauThu> CauThuDoiNha_ChuaThamGia { get; set; } = new List<CauThu>();
        public List<CauThu> CauThuDoiKhach_ChuaThamGia { get; set; } = new List<CauThu>();
        public List<string> SelectedCauThuIds { get; set; } = new List<string>();
        public int TongSoTrai_DoiNha { get; set; }
        public int TongSoTrai_DoiKhach { get; set; }

        // --- THUỘC TÍNH MỚI CHO BƯỚC NÀY ---

        // 1. Dùng để tra cứu MaCT -> MaDB
        public Dictionary<string, string> MaDoiBongLookup { get; set; } = new Dictionary<string, string>();

        // 2. Dùng để hiển thị 3 danh sách đã tham gia (thay thế cho DanhSachThamGia)
        public List<ThamGia> DanhSachThamGia_DoiNha { get; set; } = new List<ThamGia>();
        public List<ThamGia> DanhSachThamGia_DoiKhach { get; set; } = new List<ThamGia>();
        public List<ThamGia> DanhSachThamGia_Khac { get; set; } = new List<ThamGia>(); // (Dự phòng cho cầu thủ đã chuyển đội)

        public Dictionary<string, string> TenDoiBongLookup { get; set; } = new Dictionary<string, string>();
    }
}