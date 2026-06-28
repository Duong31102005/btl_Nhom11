using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface ISuCoService
    {
        Task<PagedResult<SuCoDto>> GetSuCosAsync(string? keyword, string? trangThai, int? maPhong, int pageNumber, int pageSize);
        Task<SuCoDto?> GetSuCoByIdAsync(int id);
        Task<SuCoDto> CreateSuCoAsync(CreateSuCoRequest request, int userId);
        Task<bool> UpdateSuCoStatusAsync(int id, UpdateSuCoStatusRequest request, int adminId);
        Task<bool> DeleteSuCoAsync(int id);
    }

    public class SuCoService : ISuCoService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<SuCoService> _logger;

        public SuCoService(QLPhongHocContext context, ILogger<SuCoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<SuCoDto>> GetSuCosAsync(string? keyword, string? trangThai, int? maPhong, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.SUCO
                    .AsNoTracking()
                    .Include(s => s.PhongHoc)
                    .Include(s => s.ThietBi)
                    .Include(s => s.BaoCaoNguoi)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(s => s.PhongHoc.TenPhong.ToLower().Contains(kw) || 
                                             (s.ThietBi != null && s.ThietBi.TenThietBi.ToLower().Contains(kw)) ||
                                             s.MoTaSuCo.ToLower().Contains(kw));
                }

                if (!string.IsNullOrWhiteSpace(trangThai))
                {
                    query = query.Where(s => s.TrangThai == trangThai);
                }

                if (maPhong.HasValue)
                {
                    query = query.Where(s => s.MaPhong == maPhong.Value);
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderByDescending(s => s.NgayBaoCao)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SuCoDto
                    {
                        MaSuCo = s.MaSuCo,
                        MaPhong = s.MaPhong,
                        TenPhong = s.PhongHoc != null ? s.PhongHoc.TenPhong : string.Empty,
                        MaThietBi = s.MaThietBi,
                        TenThietBi = s.ThietBi != null ? s.ThietBi.TenThietBi : null,
                        NguoiBaoCao = s.NguoiBaoCao,
                        TenNguoiBaoCao = s.BaoCaoNguoi != null ? s.BaoCaoNguoi.HoTen : string.Empty,
                        MoTaSuCo = s.MoTaSuCo,
                        NgayBaoCao = s.NgayBaoCao,
                        TrangThai = s.TrangThai
                    })
                    .ToListAsync();

                return new PagedResult<SuCoDto>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetSuCosAsync");
                throw;
            }
        }

        public async Task<SuCoDto?> GetSuCoByIdAsync(int id)
        {
            try
            {
                var s = await _context.SUCO
                    .AsNoTracking()
                    .Include(x => x.PhongHoc)
                    .Include(x => x.ThietBi)
                    .Include(x => x.BaoCaoNguoi)
                    .FirstOrDefaultAsync(x => x.MaSuCo == id);

                if (s == null) return null;

                return new SuCoDto
                {
                    MaSuCo = s.MaSuCo,
                    MaPhong = s.MaPhong,
                    TenPhong = s.PhongHoc != null ? s.PhongHoc.TenPhong : string.Empty,
                    MaThietBi = s.MaThietBi,
                    TenThietBi = s.ThietBi != null ? s.ThietBi.TenThietBi : null,
                    NguoiBaoCao = s.NguoiBaoCao,
                    TenNguoiBaoCao = s.BaoCaoNguoi != null ? s.BaoCaoNguoi.HoTen : string.Empty,
                    MoTaSuCo = s.MoTaSuCo,
                    NgayBaoCao = s.NgayBaoCao,
                    TrangThai = s.TrangThai
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong GetSuCoByIdAsync cho MaSuCo: {id}");
                throw;
            }
        }

        public async Task<SuCoDto> CreateSuCoAsync(CreateSuCoRequest request, int userId)
        {
            try
            {
                // Kiểm tra phòng học tồn tại
                var phong = await _context.PHONGHOC.FindAsync(request.MaPhong);
                if (phong == null)
                {
                    throw new InvalidOperationException("Phòng học báo cáo sự cố không tồn tại.");
                }

                ThietBi? thietBi = null;
                if (request.MaThietBi.HasValue)
                {
                    thietBi = await _context.THIETBI.FindAsync(request.MaThietBi.Value);
                    if (thietBi == null || thietBi.MaPhong != request.MaPhong)
                    {
                        throw new InvalidOperationException("Thiết bị báo cáo sự cố không tồn tại hoặc không thuộc phòng học này.");
                    }
                }

                var suCo = new SuCo
                {
                    MaPhong = request.MaPhong,
                    MaThietBi = request.MaThietBi,
                    NguoiBaoCao = userId,
                    MoTaSuCo = request.MoTaSuCo,
                    NgayBaoCao = DateTime.Now,
                    TrangThai = "Chờ xử lý"
                };

                _context.SUCO.Add(suCo);

                // NGHIỆP VỤ TỰ ĐỘNG: Chuyển trạng thái thiết bị hoặc phòng học sang 'Cần sửa chữa'
                if (thietBi != null)
                {
                    thietBi.TinhTrang = "Cần sửa chữa";
                    _context.THIETBI.Update(thietBi);
                    _logger.LogInformation($"Tự động cập nhật trạng thái thiết bị {thietBi.TenThietBi} thành 'Cần sửa chữa'.");
                }
                
                // Cập nhật luôn phòng học sang 'Cần sửa chữa' nếu sự cố phòng học hoặc thiết bị trong phòng hỏng nặng
                phong.TrangThai = "Cần sửa chữa";
                _context.PHONGHOC.Update(phong);
                _logger.LogInformation($"Tự động cập nhật trạng thái phòng {phong.TenPhong} thành 'Cần sửa chữa'.");

                await _context.SaveChangesAsync();

                // Ghi log vào LICHSU
                var log = new LichSu
                {
                    LoaiLichSu = "SuCo",
                    NoiDung = $"Báo cáo sự cố mới tại phòng {phong.TenPhong}: {request.MoTaSuCo}. {(thietBi != null ? $"Thiết bị ảnh hưởng: {thietBi.TenThietBi}" : "")}",
                    NguoiThucHien = userId
                };
                _context.LICHSU.Add(log);
                await _context.SaveChangesAsync();

                return await GetSuCoByIdAsync(suCo.MaSuCo) ?? new SuCoDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong CreateSuCoAsync");
                throw;
            }
        }

        public async Task<bool> UpdateSuCoStatusAsync(int id, UpdateSuCoStatusRequest request, int adminId)
        {
            try
            {
                var suCo = await _context.SUCO.FindAsync(id);
                if (suCo == null) return false;

                suCo.TrangThai = request.TrangThai;
                _context.SUCO.Update(suCo);

                // Ghi log vào LICHSU
                var log = new LichSu
                {
                    LoaiLichSu = "SuCo",
                    NoiDung = $"Cập nhật trạng thái sự cố mã {id} thành '{request.TrangThai}'",
                    NguoiThucHien = adminId
                };
                _context.LICHSU.Add(log);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cập nhật trạng thái sự cố {id} thành {request.TrangThai}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong UpdateSuCoStatusAsync cho MaSuCo: {id}");
                throw;
            }
        }

        public async Task<bool> DeleteSuCoAsync(int id)
        {
            try
            {
                var suCo = await _context.SUCO.FindAsync(id);
                if (suCo == null) return false;

                _context.SUCO.Remove(suCo);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Xóa sự cố mã {id} thành công.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong DeleteSuCoAsync cho MaSuCo: {id}");
                throw;
            }
        }
    }
}
