using System;
using System.ComponentModel.DataAnnotations;

namespace CSDLPT.Web.Models
{
    public class DoiBong : IValidatableObject
    {
        [Required, MaxLength(20)] public string MaDB { get; set; } = "";
        [Required, MaxLength(100)] public string TenDB { get; set; } = "";
        [MaxLength(20)] public string? MaSan { get; set; }
        [MaxLength(100)] public string? HLV { get; set; }
        [Required, MaxLength(10)] public string CLB { get; set; } = "";

        [Display(Name = "Node")]
        public string? Node { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            string? expectedPrefix = CLB switch
            {
                "CLB1" => "A_",
                "CLB2" => "B_",
                "CLB3" => "C_",
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(MaDB) && !string.IsNullOrWhiteSpace(CLB) && expectedPrefix != null)
            {
                if (!MaDB.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult(
                        $"MaDB phải bắt đầu bằng \"{expectedPrefix}\" theo {CLB}.",
                        new[] { nameof(MaDB) });
                }
            }
        }
    }

    public class CauThu // 1. Xóa bỏ ": IValidatableObject"
    {
        // 2. Bỏ [Required] khỏi MaCT
        [MaxLength(20)]
        [Display(Name = "Mã Cầu Thủ")]
        public string MaCT { get; set; } = ""; // Sẽ được CSDL tự điền

        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [MaxLength(100)]
        [Display(Name = "Họ Tên")]
        public string HoTen { get; set; } = "";

        [MaxLength(50)]
        [Display(Name = "Vị Trí")]
        public string? ViTri { get; set; }

        [Display(Name = "Ngày Sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Mã đội bóng là bắt buộc.")]
        [MaxLength(20)]
        [Display(Name = "Mã Đội Bóng")]
        public string MaDB { get; set; } = ""; // Vẫn [Required]

        [Display(Name = "Số Áo")]
        public int? SoAo { get; set; }

        [MaxLength(50)]
        [Display(Name = "Quốc Tịch")]
        public string? QuocTich { get; set; }

        [Display(Name = "Node")]
        public string? Node { get; set; }

        // 3. XÓA BỎ toàn bộ phương thức Validate (IValidatableObject)
        /*
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // XÓA HẾT CODE TRONG NÀY
        }
        */
    }

    public class TranDau
    {
        [Required, MaxLength(20)] public string MaTD { get; set; } = "";
        public DateTime? NgayThiDau { get; set; }
        [Required, MaxLength(20)] public string MaSan { get; set; } = "";
        public int? LuotDau { get; set; }
        public int? VongDau { get; set; }
    }

    public class ThamGia
    {
        [Required, MaxLength(20)] public string MaTD { get; set; } = "";
        [Required, MaxLength(20)] public string MaCT { get; set; } = "";
        public int? SoTrai { get; set; }
    }
}
