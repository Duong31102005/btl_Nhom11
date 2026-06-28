using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IBaoTriService
    {
        Task<PagedResult<BaoTriDto>> GetBaoTrisAsync(string? keyword, string? ketQua, int? maKyThuatVien, int pageNumber, int pageSize);
        Task<BaoTriDto?> GetBaoTriByIdAsync(int id);
        Task<BaoTriDto> CreateBaoTriAsync(CreateBaoTriRequest request);
        Task<BaoTriDto?> UpdateBaoTriAsync(int id, UpdateBaoTriRequest request, int updatedByUserId);
        Task<bool> DeleteBaoTriAsync(int id);
    }

    public class BaoTriService : IBaoTriService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<BaoTriService> _logger;

        public BaoTriService(QLPhongHocContext context, ILogger<BaoTriService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<BaoTriDto>> GetBaoTrisAsync(string? keyword, string? ketQua, int? maKyThuatVien, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.BAOTRI
                    .AsNoTracking()
                    .Include(b => b.SuCo)
                        .ThenInclude(s => s.PhongHoc)
                    .Include(b => b.SuCo)
                        .ThenInclude(s => s.ThietBi)
                    .Include(b => b.KyThuatVien)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(b => b.SuCo.PhongHoc.TenPhong.ToLower().Contains(kw) || 
                                             b.NoiDungXuLy.ToLower().Contains(kw) ||
                                             b.KyThuatVien.HoTen.ToLower().Contains(kw));
                }

                if (!string.IsNullOrWhiteSpace(ketQua))
                {
                    query = query.Where(b => b.KetQua == ketQua);
                }

                if (maKyThuatVien.HasValue)
                {
                    query = query.Where(b => b.MaKyThuatVien == maKyThuatVien.Value);
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderByDescending(b => b.NgayXuLy)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new BaoTriDto
                    {
                        MaBaoTri = b.MaBaoTri,
                        MaSuCo = b.MaSuCo,
                        MoTaSuCo = b.SuCo.MoTaSuCo,
                        TenPhong = b.SuCo.PhongHoc.TenPhong,
                        TenThietBi = b.SuCo.ThietBi != null ? b.SuCo.ThietBi.TenThietBi : null,
                        MaKyThuatVien = b.MaKyThuatVien,
                        TenKyThuatVien = b.KyThuatVien.HoTen,
                        NgayXuLy = b.NgayXuLy,
                        NoiDungXuLy = b.NoiDungXuLy,
                        KetQua = b.KetQua
                    })
                    .ToListAsync();

                return new PagedResult<BaoTriDto>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetBaoTrisAsync");
                throw;
            }
        }

        public async Task<BaoTriDto?> GetBaoTriByIdAsync(int id)
        {
            try
            {
                var b = await _context.BAOTRI
                    .AsNoTracking()
                    .Include(x => x.SuCo)
                        .ThenInclude(s => s.PhongHoc)
                    .Include(x => x.SuCo)
                        .ThenInclude(s => s.ThietBi)
                    .Include(x => x.KyThuatVien)
                    .FirstOrDefaultAsync(x => x.MaBaoTri == id);

                if (b == null) return null;

                return new BaoTriDto
                {
                    MaBaoTri = b.MaBaoTri,
                    MaSuCo = b.MaSuCo,
                    MoTaSuCo = b.SuCo.MoTaSuCo,
                    TenPhong = b.SuCo.PhongHoc.TenPhong,
                    TenThietBi = b.SuCo.ThietBi != null ? b.SuCo.ThietBi.TenThietBi : null,
                    MaKyThuatVien = b.MaKyThuatVien,
                    TenKyThuatVien = b.KyThuatVien.HoTen,
                    NgayXuLy = b.NgayXuLy,
                    NoiDungXuLy = b.NoiDungXuLy,
                    KetQua = b.KetQua
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong GetBaoTriByIdAsync cho MaBaoTri: {id}");
                throw;
            }
        }

        public async Task<BaoTriDto> CreateBaoTriAsync(CreateBaoTriRequest request)
        {
            try
            {
                // Kiểm tra sự cố tồn tại
                var suCo = await _context.SUCO.FindAsync(request.MaSuCo);
                if (suCo == null)
                {
                    throw new InvalidOperationException("Sự cố báo cáo cần sửa chữa không tồn tại.");
                }

                // Kiểm tra kỹ thuật viên tồn tại và đúng vai trò
                var ktv = await _context.TAIKHOAN
                    .Include(t => t.VaiTro)
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == request.MaKyThuatVien);
                if (ktv == null || ktv.VaiTro?.TenVaiTro != "Kỹ thuật viên")
                {
                    throw new InvalidOperationException("Kỹ thuật viên phân công không tồn tại hoặc không có vai trò phù hợp.");
                }

                var baoTri = new BaoTri
                {
                    MaSuCo = request.MaSuCo,
                    MaKyThuatVien = request.MaKyThuatVien,
                    NgayXuLy = DateTime.Now,
                    NoiDungXuLy = request.NoiDungXuLy,
                    KetQua = "Đang sửa"
                };

                _context.BAOTRI.Add(baoTri);

                // Cập nhật trạng thái sự cố sang 'Đang xử lý'
                suCo.TrangThai = "Đang xử lý";
                _context.SUCO.Update(suCo);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đã lên lịch bảo trì cho sự cố {request.MaSuCo}, kỹ thuật viên: {ktv.HoTen}");

                return await GetBaoTriByIdAsync(baoTri.MaBaoTri) ?? new BaoTriDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong CreateBaoTriAsync");
                throw;
            }
        }

        public async Task<BaoTriDto?> UpdateBaoTriAsync(int id, UpdateBaoTriRequest request, int updatedByUserId)
        {
            try
            {
                var b = await _context.BAOTRI
                    .Include(x => x.SuCo)
                        .ThenInclude(s => s.PhongHoc)
                    .Include(x => x.SuCo)
                        .ThenInclude(s => s.ThietBi)
                    .Include(x => x.KyThuatVien)
                    .FirstOrDefaultAsync(x => x.MaBaoTri == id);

                if (b == null) return null;

                b.NoiDungXuLy = request.NoiDungXuLy;
                b.KetQua = request.KetQua;
                b.NgayXuLy = DateTime.Now;

                _context.BAOTRI.Update(b);

                // NGHIỆP VỤ TỰ ĐỘNG: Khi hoàn thành bảo trì thành công
                if (request.KetQua == "Thành công")
                {
                    // 1. Cập nhật trạng thái sự cố thành 'Đã khắc phục'
                    b.SuCo.TrangThai = "Đã khắc phục";
                    _context.SUCO.Update(b.SuCo);

                    // 2. Cập nhật trạng thái thiết bị thành 'Hoạt động' nếu sự cố liên quan thiết bị
                    if (b.SuCo.MaThietBi.HasValue && b.SuCo.ThietBi != null)
                    {
                        b.SuCo.ThietBi.TinhTrang = "Hoạt động";
                        _context.THIETBI.Update(b.SuCo.ThietBi);
                        _logger.LogInformation($"Bảo trì thành công: Đưa thiết bị {b.SuCo.ThietBi.TenThietBi} trở lại hoạt động.");
                    }

                    // 3. Kiểm tra xem phòng học có còn sự cố nào khác chưa xử lý không
                    var hasOtherActiveIncidents = await _context.SUCO.AnyAsync(s => 
                        s.MaPhong == b.SuCo.MaPhong && 
                        s.MaSuCo != b.MaSuCo && 
                        s.TrangThai != "Đã khắc phục");

                    if (!hasOtherActiveIncidents)
                    {
                        // Đưa phòng học trở lại hoạt động bình thường
                        b.SuCo.PhongHoc.TrangThai = "Hoạt động";
                        _context.PHONGHOC.Update(b.SuCo.PhongHoc);
                        _logger.LogInformation($"Bảo trì thành công: Đưa phòng {b.SuCo.PhongHoc.TenPhong} trở lại hoạt động bình thường.");
                    }

                    // 4. Ghi log hoạt động vào bảng LICHSU
                    var log = new LichSu
                    {
                        LoaiLichSu = "BaoTri",
                        NoiDung = $"Hoàn thành bảo trì sự cố mã {b.MaSuCo} tại phòng {b.SuCo.PhongHoc.TenPhong}. Kết quả: Thành công. Kỹ thuật viên: {b.KyThuatVien.HoTen}.",
                        NguoiThucHien = updatedByUserId
                    };
                    _context.LICHSU.Add(log);
                }
                else if (request.KetQua == "Thất bại")
                {
                    b.SuCo.TrangThai = "Không khắc phục được";
                    _context.SUCO.Update(b.SuCo);

                    // Nếu thiết bị hỏng nặng không thể sửa, chuyển tình trạng thiết bị thành Hỏng hoặc Ngừng sử dụng
                    if (b.SuCo.MaThietBi.HasValue && b.SuCo.ThietBi != null)
                    {
                        b.SuCo.ThietBi.TinhTrang = "Ngừng sử dụng";
                        _context.THIETBI.Update(b.SuCo.ThietBi);
                    }

                    // Ghi log thất bại vào LICHSU
                    var log = new LichSu
                    {
                        LoaiLichSu = "BaoTri",
                        NoiDung = $"Bảo trì thất bại đối với sự cố mã {b.MaSuCo} tại phòng {b.SuCo.PhongHoc.TenPhong}. Kỹ thuật viên: {b.KyThuatVien.HoTen}.",
                        NguoiThucHien = updatedByUserId
                    };
                    _context.LICHSU.Add(log);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cập nhật kết quả bảo trì mã {id} thành {request.KetQua}");

                return await GetBaoTriByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong UpdateBaoTriAsync cho MaBaoTri: {id}");
                throw;
            }
        }

        public async Task<bool> DeleteBaoTriAsync(int id)
        {
            try
            {
                var b = await _context.BAOTRI.FindAsync(id);
                if (b == null) return false;

                _context.BAOTRI.Remove(b);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Xóa lịch bảo trì mã {id} thành công.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong DeleteBaoTriAsync cho MaBaoTri: {id}");
                throw;
            }
        }
    }
}
