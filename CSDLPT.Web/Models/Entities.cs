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

    public class CauThu
    {
        [Required, MaxLength(20)] public string MaCT { get; set; } = "";
        [Required, MaxLength(100)] public string HoTen { get; set; } = "";
        [MaxLength(50)] public string? ViTri { get; set; }
        public DateTime? NgaySinh { get; set; }
        [Required, MaxLength(20)] public string MaDB { get; set; } = "";
        public int? SoAo { get; set; }
        [MaxLength(50)] public string? QuocTich { get; set; }
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
