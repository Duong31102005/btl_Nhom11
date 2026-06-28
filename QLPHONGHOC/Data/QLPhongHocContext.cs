using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Models;

namespace QLPhongHoc.Data
{
    public class QLPhongHocContext : DbContext
    {
        public QLPhongHocContext(DbContextOptions<QLPhongHocContext> options) : base(options)
        {
        }

        public DbSet<VaiTro> VAITRO { get; set; }
        public DbSet<TaiKhoan> TAIKHOAN { get; set; }
        public DbSet<PhongHoc> PHONGHOC { get; set; }
        public DbSet<ThietBi> THIETBI { get; set; }
        public DbSet<YeuCauSuDungPhong> YEUCAUSUDUNGPHONG { get; set; }
        public DbSet<LichSuDungPhong> LICHSUDUNGPHONG { get; set; }
        public DbSet<SuCo> SUCO { get; set; }
        public DbSet<BaoTri> BAOTRI { get; set; }
        public DbSet<LichSu> LICHSU { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng VAITRO
            modelBuilder.Entity<VaiTro>()
                .ToTable("VAITRO");

            // Cấu hình bảng TAIKHOAN
            modelBuilder.Entity<TaiKhoan>()
                .ToTable("TAIKHOAN");
            
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.VaiTro)
                .WithMany()
                .HasForeignKey(t => t.MaVaiTro)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng PHONGHOC
            modelBuilder.Entity<PhongHoc>()
                .ToTable("PHONGHOC");

            // Cấu hình bảng THIETBI
            modelBuilder.Entity<ThietBi>()
                .ToTable("THIETBI")
                .HasOne(tb => tb.PhongHoc)
                .WithMany(p => p.ThietBis)
                .HasForeignKey(tb => tb.MaPhong)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình bảng DANGKY (YeuCauSuDungPhong)
            modelBuilder.Entity<YeuCauSuDungPhong>()
                .ToTable("DANGKY")
                .HasOne(y => y.GiangVien)
                .WithMany()
                .HasForeignKey(y => y.MaGiangVien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<YeuCauSuDungPhong>()
                .HasOne(y => y.PhongHoc)
                .WithMany()
                .HasForeignKey(y => y.MaPhong)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng SUCO
            modelBuilder.Entity<SuCo>()
                .ToTable("SUCO");

            modelBuilder.Entity<SuCo>()
                .HasOne(s => s.PhongHoc)
                .WithMany()
                .HasForeignKey(s => s.MaPhong)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SuCo>()
                .HasOne(s => s.ThietBi)
                .WithMany()
                .HasForeignKey(s => s.MaThietBi)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SuCo>()
                .HasOne(s => s.BaoCaoNguoi)
                .WithMany()
                .HasForeignKey(s => s.NguoiBaoCao)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng BAOTRI
            modelBuilder.Entity<BaoTri>()
                .ToTable("BAOTRI")
                .HasOne(b => b.SuCo)
                .WithMany()
                .HasForeignKey(b => b.MaSuCo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BaoTri>()
                .HasOne(b => b.KyThuatVien)
                .WithMany()
                .HasForeignKey(b => b.MaKyThuatVien)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng LICHSU
            modelBuilder.Entity<LichSu>()
                .ToTable("LICHSU")
                .HasOne(l => l.NguoiThucHienTaiKhoan)
                .WithMany()
                .HasForeignKey(l => l.NguoiThucHien)
                .OnDelete(DeleteBehavior.SetNull);

            // Cấu hình bảng LICHSUDUNGPHONG
            modelBuilder.Entity<LichSuDungPhong>()
                .ToTable("LICHSUDUNGPHONG")
                .HasOne(l => l.PhongHoc)
                .WithMany(p => p.LichSuDungPhongs)
                .HasForeignKey(l => l.MaPhong)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LichSuDungPhong>()
                .HasOne(l => l.GiangVien)
                .WithMany()
                .HasForeignKey(l => l.MaGiangVien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuDungPhong>()
                .HasOne(l => l.YeuCau)
                .WithMany()
                .HasForeignKey(l => l.MaYeuCau)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
