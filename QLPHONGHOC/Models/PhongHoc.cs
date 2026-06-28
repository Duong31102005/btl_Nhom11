using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("PHONGHOC")]
    public class PhongHoc
    {
        [Key]
        public int MaPhong { get; set; }

        [StringLength(50)]
        public string TenPhong { get; set; } = string.Empty;

        [StringLength(50)]
        public string DayNha { get; set; } = string.Empty;

        public int Tang { get; set; }

        public int SucChua { get; set; }

        [StringLength(50)]
        public string LoaiPhong { get; set; } = string.Empty;

        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        [StringLength(255)]
        public string? GhiChu { get; set; }

        public ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
        public ICollection<LichSuDungPhong> LichSuDungPhongs { get; set; } = new List<LichSuDungPhong>();
    }
}
