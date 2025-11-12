/* Nội dung file: CSDLPT.Web/Models/TranDauDetailsViewModel.cs 
Mục đích: ViewModel cho trang Details của TranDau
*/
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSDLPT.Web.Models
{
    public class TranDauDetailsViewModel
    {
        public TranDau TranDau { get; set; }
        public IEnumerable<ThamGia> DanhSachThamGia { get; set; }

        // Dùng để hiển thị tên cầu thủ (lookup)
        public Dictionary<string, string> TenCauThuLookup { get; set; }

        // Dùng cho form "Thêm mới"
        public IEnumerable<SelectListItem> CauThuOptions { get; set; }

        // Model con để binding form thêm mới
        public ThamGia NewThamGia { get; set; }

        public TranDauDetailsViewModel()
        {
            TranDau = new TranDau();
            DanhSachThamGia = new List<ThamGia>();
            TenCauThuLookup = new Dictionary<string, string>();
            CauThuOptions = new List<SelectListItem>();
            NewThamGia = new ThamGia();
        }
    }
}