using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("LICHSUDUNGPHONG")]
    public class LichSuDungPhong
    {
        [Key]
        public int MaLich { get; set; }

        public int MaPhong { get; set; }
        [ForeignKey("MaPhong")]
        public PhongHoc PhongHoc { get; set; }

        public int MaGiangVien { get; set; }
        [ForeignKey("MaGiangVien")]
        public TaiKhoan GiangVien { get; set; }

        public int? MaYeuCau { get; set; }
        [ForeignKey("MaYeuCau")]
        public YeuCauSuDungPhong YeuCau { get; set; }

        public DateTime NgaySuDung { get; set; }
        public int TietBatDau { get; set; }
        public int TietKetThuc { get; set; }

        [StringLength(255)]
        public string? NoiDung { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }
    }
}
