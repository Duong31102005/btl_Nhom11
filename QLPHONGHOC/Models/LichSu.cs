using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("LICHSU")]
    public class LichSu
    {
        [Key]
        public int MaLichSu { get; set; }

        [Required]
        [StringLength(50)]
        public string LoaiLichSu { get; set; } // 'BaoTri', 'DangKy', 'SuCo', 'TaiKhoan'

        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiThucHien { get; set; }

        [ForeignKey("NguoiThucHien")]
        public virtual TaiKhoan? NguoiThucHienTaiKhoan { get; set; }
    }
}
