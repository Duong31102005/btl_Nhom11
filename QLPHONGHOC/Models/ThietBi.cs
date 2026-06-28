using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("THIETBI")]
    public class ThietBi
    {
        [Key]
        public int MaThietBi { get; set; }

        [StringLength(100)]
        public string TenThietBi { get; set; } = string.Empty;

        public int MaPhong { get; set; }
        [ForeignKey("MaPhong")]
        public PhongHoc PhongHoc { get; set; } = null!;

        public int SoLuong { get; set; }

        [StringLength(50)]
        public string TinhTrang { get; set; } = string.Empty;

        [StringLength(255)]
        public string? GhiChu { get; set; }
    }
}
