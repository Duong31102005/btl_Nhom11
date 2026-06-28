using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("BAOTRI")]
    public class BaoTri
    {
        [Key]
        public int MaBaoTri { get; set; }

        public int MaSuCo { get; set; }
        [ForeignKey("MaSuCo")]
        public SuCo SuCo { get; set; }

        public int MaKyThuatVien { get; set; }
        [ForeignKey("MaKyThuatVien")]
        public TaiKhoan KyThuatVien { get; set; }

        public DateTime NgayXuLy { get; set; }

        [StringLength(255)]
        public string NoiDungXuLy { get; set; }

        [StringLength(100)]
        public string KetQua { get; set; }
    }
}
