using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("DANGKY")]
    public class YeuCauSuDungPhong
    {
        [Key]
        public int MaYeuCau { get; set; }

        public int MaGiangVien { get; set; }
        [ForeignKey("MaGiangVien")]
        public TaiKhoan GiangVien { get; set; }

        public int MaPhong { get; set; }
        [ForeignKey("MaPhong")]
        public PhongHoc PhongHoc { get; set; }

        public DateTime NgaySuDung { get; set; }
        public int TietBatDau { get; set; }
        public int TietKetThuc { get; set; }

        [StringLength(255)]
        public string? MucDich { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }

        [StringLength(255)]
        public string? LyDoTuChoi { get; set; }

        public DateTime NgayTao { get; set; }
    }
}
