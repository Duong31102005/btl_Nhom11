using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IThietBiService
    {
        Task<PagedResult<ThietBiDto>> GetThietBisAsync(string? keyword, int? maPhong, string? tinhTrang, int pageNumber, int pageSize);
        Task<ThietBiDto?> GetThietBiByIdAsync(int id);
        Task<ThietBiDto> CreateThietBiAsync(CreateThietBiRequest request);
        Task<ThietBiDto?> UpdateThietBiAsync(int id, UpdateThietBiRequest request);
        Task<bool> DeleteThietBiAsync(int id);
    }

    public class ThietBiService : IThietBiService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<ThietBiService> _logger;

        public ThietBiService(QLPhongHocContext context, ILogger<ThietBiService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<ThietBiDto>> GetThietBisAsync(string? keyword, int? maPhong, string? tinhTrang, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.THIETBI.AsNoTracking().Include(t => t.PhongHoc).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(t => t.TenThietBi.ToLower().Contains(kw));
                }

                if (maPhong.HasValue)
                {
                    query = query.Where(t => t.MaPhong == maPhong.Value);
                }

                if (!string.IsNullOrWhiteSpace(tinhTrang))
                {
                    query = query.Where(t => t.TinhTrang == tinhTrang);
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderBy(t => t.MaThietBi)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new ThietBiDto
                    {
                        MaThietBi = t.MaThietBi,
                        TenThietBi = t.TenThietBi,
                        MaPhong = t.MaPhong,
                        TenPhong = t.PhongHoc != null ? t.PhongHoc.TenPhong : string.Empty,
                        SoLuong = t.SoLuong,
                        TinhTrang = t.TinhTrang,
                        GhiChu = t.GhiChu
                    })
                    .ToListAsync();

                return new PagedResult<ThietBiDto>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetThietBisAsync");
                throw;
            }
        }

        public async Task<ThietBiDto?> GetThietBiByIdAsync(int id)
        {
            try
            {
                var t = await _context.THIETBI
                    .AsNoTracking()
                    .Include(x => x.PhongHoc)
                    .FirstOrDefaultAsync(x => x.MaThietBi == id);

                if (t == null) return null;

                return new ThietBiDto
                {
                    MaThietBi = t.MaThietBi,
                    TenThietBi = t.TenThietBi,
                    MaPhong = t.MaPhong,
                    TenPhong = t.PhongHoc != null ? t.PhongHoc.TenPhong : string.Empty,
                    SoLuong = t.SoLuong,
                    TinhTrang = t.TinhTrang,
                    GhiChu = t.GhiChu
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong GetThietBiByIdAsync cho MaThietBi: {id}");
                throw;
            }
        }

        public async Task<ThietBiDto> CreateThietBiAsync(CreateThietBiRequest request)
        {
            try
            {
                // Kiểm tra phòng học tồn tại
                var roomExists = await _context.PHONGHOC.AnyAsync(p => p.MaPhong == request.MaPhong);
                if (!roomExists)
                {
                    throw new InvalidOperationException($"Phòng học với mã {request.MaPhong} không tồn tại.");
                }

                var thietBi = new ThietBi
                {
                    TenThietBi = request.TenThietBi,
                    MaPhong = request.MaPhong,
                    SoLuong = request.SoLuong,
                    TinhTrang = request.TinhTrang,
                    GhiChu = request.GhiChu
                };

                _context.THIETBI.Add(thietBi);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Thêm mới thiết bị thành công: {thietBi.TenThietBi}");

                return await GetThietBiByIdAsync(thietBi.MaThietBi) ?? new ThietBiDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong CreateThietBiAsync");
                throw;
            }
        }

        public async Task<ThietBiDto?> UpdateThietBiAsync(int id, UpdateThietBiRequest request)
        {
            try
            {
                var thietBi = await _context.THIETBI.FindAsync(id);
                if (thietBi == null) return null;

                // Kiểm tra phòng học tồn tại
                var roomExists = await _context.PHONGHOC.AnyAsync(p => p.MaPhong == request.MaPhong);
                if (!roomExists)
                {
                    throw new InvalidOperationException($"Phòng học với mã {request.MaPhong} không tồn tại.");
                }

                thietBi.TenThietBi = request.TenThietBi;
                thietBi.MaPhong = request.MaPhong;
                thietBi.SoLuong = request.SoLuong;
                thietBi.TinhTrang = request.TinhTrang;
                thietBi.GhiChu = request.GhiChu;

                _context.THIETBI.Update(thietBi);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cập nhật thiết bị thành công: {thietBi.TenThietBi}");

                return await GetThietBiByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong UpdateThietBiAsync cho MaThietBi: {id}");
                throw;
            }
        }

        public async Task<bool> DeleteThietBiAsync(int id)
        {
            try
            {
                var thietBi = await _context.THIETBI.FindAsync(id);
                if (thietBi == null) return false;

                // Kiểm tra xem thiết bị có liên kết với sự cố chưa giải quyết không
                var hasActiveIncidents = await _context.SUCO.AnyAsync(s => s.MaThietBi == id && s.TrangThai != "Đã khắc phục");
                if (hasActiveIncidents)
                {
                    // Chuyển tình trạng thiết bị thành Ngừng sử dụng
                    thietBi.TinhTrang = "Ngừng sử dụng";
                    _context.THIETBI.Update(thietBi);
                    _logger.LogInformation($"Thiết bị {thietBi.TenThietBi} có liên quan đến sự cố chưa khắc phục, chuyển trạng thái sang 'Ngừng sử dụng'.");
                }
                else
                {
                    // Xóa cứng
                    _context.THIETBI.Remove(thietBi);
                    _logger.LogInformation($"Xóa cứng thiết bị {thietBi.TenThietBi} thành công.");
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong DeleteThietBiAsync cho MaThietBi: {id}");
                throw;
            }
        }
    }
}
