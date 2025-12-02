using System;

namespace CSDLPT.Web.Models
{
    public class LichSuThiDauViewModel
    {
        public string MaTD { get; set; }
        public DateTime? NgayThiDau { get; set; }
        public string MaSan { get; set; }
        public int SoBanThang { get; set; }

        // Thông tin cầu thủ (để hiển thị lại cho rõ)
        public string MaCT { get; set; }
        public string Hoten { get; set; }
    }
}