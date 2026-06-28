using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("SUCO")]
    public class SuCo
    {
        [Key]
        public int MaSuCo { get; set; }

        public int MaPhong { get; set; }
        [ForeignKey("MaPhong")]
        public PhongHoc PhongHoc { get; set; }

        public int? MaThietBi { get; set; }
        [ForeignKey("MaThietBi")]
        public ThietBi ThietBi { get; set; }

        public int NguoiBaoCao { get; set; }
        [ForeignKey("NguoiBaoCao")]
        public TaiKhoan BaoCaoNguoi { get; set; }

        [StringLength(255)]
        public string? MoTaSuCo { get; set; }

        public DateTime NgayBaoCao { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }
    }
}
