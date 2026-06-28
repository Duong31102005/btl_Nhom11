using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IPhongHocService
    {
        Task<PagedResult<PhongHocDto>> GetPhongHocsAsync(string? keyword, string? loaiPhong, string? trangThai, int pageNumber, int pageSize);
        Task<PhongHocDto?> GetPhongHocByIdAsync(int id);
        Task<PhongHocDto> CreatePhongHocAsync(CreatePhongHocRequest request);
        Task<PhongHocDto?> UpdatePhongHocAsync(int id, UpdatePhongHocRequest request);
        Task<bool> DeletePhongHocAsync(int id);
    }

    public class PhongHocService : IPhongHocService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<PhongHocService> _logger;

        public PhongHocService(QLPhongHocContext context, ILogger<PhongHocService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<PhongHocDto>> GetPhongHocsAsync(string? keyword, string? loaiPhong, string? trangThai, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.PHONGHOC.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(p => p.TenPhong.ToLower().Contains(kw) || p.DayNha.ToLower().Contains(kw));
                }

                if (!string.IsNullOrWhiteSpace(loaiPhong))
                {
                    query = query.Where(p => p.LoaiPhong == loaiPhong);
                }

                if (!string.IsNullOrWhiteSpace(trangThai))
                {
                    query = query.Where(p => p.TrangThai == trangThai);
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderBy(p => p.DayNha)
                    .ThenBy(p => p.TenPhong)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PhongHocDto
                    {
                        MaPhong = p.MaPhong,
                        TenPhong = p.TenPhong,
                        DayNha = p.DayNha,
                        Tang = p.Tang,
                        SucChua = p.SucChua,
                        LoaiPhong = p.LoaiPhong,
                        TrangThai = p.TrangThai,
                        GhiChu = p.GhiChu
                    })
                    .ToListAsync();

                return new PagedResult<PhongHocDto>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetPhongHocsAsync");
                throw;
            }
        }

        public async Task<PhongHocDto?> GetPhongHocByIdAsync(int id)
        {
            try
            {
                var p = await _context.PHONGHOC.AsNoTracking().FirstOrDefaultAsync(x => x.MaPhong == id);
                if (p == null) return null;

                return new PhongHocDto
                {
                    MaPhong = p.MaPhong,
                    TenPhong = p.TenPhong,
                    DayNha = p.DayNha,
                    Tang = p.Tang,
                    SucChua = p.SucChua,
                    LoaiPhong = p.LoaiPhong,
                    TrangThai = p.TrangThai,
                    GhiChu = p.GhiChu
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong GetPhongHocByIdAsync cho MaPhong: {id}");
                throw;
            }
        }

        public async Task<PhongHocDto> CreatePhongHocAsync(CreatePhongHocRequest request)
        {
            try
            {
                // Kiểm tra tên phòng độc nhất
                var exists = await _context.PHONGHOC.AnyAsync(p => p.TenPhong == request.TenPhong);
                if (exists)
                {
                    throw new InvalidOperationException($"Phòng học với tên {request.TenPhong} đã tồn tại.");
                }

                var phong = new PhongHoc
                {
                    TenPhong = request.TenPhong,
                    DayNha = request.DayNha,
                    Tang = request.Tang,
                    SucChua = request.SucChua,
                    LoaiPhong = request.LoaiPhong,
                    TrangThai = request.TrangThai,
                    GhiChu = request.GhiChu
                };

                _context.PHONGHOC.Add(phong);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Thêm mới phòng học thành công: {phong.TenPhong}");

                return new PhongHocDto
                {
                    MaPhong = phong.MaPhong,
                    TenPhong = phong.TenPhong,
                    DayNha = phong.DayNha,
                    Tang = phong.Tang,
                    SucChua = phong.SucChua,
                    LoaiPhong = phong.LoaiPhong,
                    TrangThai = phong.TrangThai,
                    GhiChu = phong.GhiChu
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong CreatePhongHocAsync");
                throw;
            }
        }

        public async Task<PhongHocDto?> UpdatePhongHocAsync(int id, UpdatePhongHocRequest request)
        {
            try
            {
                var phong = await _context.PHONGHOC.FindAsync(id);
                if (phong == null) return null;

                // Kiểm tra trùng tên phòng
                var exists = await _context.PHONGHOC.AnyAsync(p => p.TenPhong == request.TenPhong && p.MaPhong != id);
                if (exists)
                {
                    throw new InvalidOperationException($"Phòng học với tên {request.TenPhong} đã tồn tại.");
                }

                phong.TenPhong = request.TenPhong;
                phong.DayNha = request.DayNha;
                phong.Tang = request.Tang;
                phong.SucChua = request.SucChua;
                phong.LoaiPhong = request.LoaiPhong;
                phong.TrangThai = request.TrangThai;
                phong.GhiChu = request.GhiChu;

                _context.PHONGHOC.Update(phong);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cập nhật phòng học thành công: {phong.TenPhong}");

                return new PhongHocDto
                {
                    MaPhong = phong.MaPhong,
                    TenPhong = phong.TenPhong,
                    DayNha = phong.DayNha,
                    Tang = phong.Tang,
                    SucChua = phong.SucChua,
                    LoaiPhong = phong.LoaiPhong,
                    TrangThai = phong.TrangThai,
                    GhiChu = phong.GhiChu
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong UpdatePhongHocAsync cho MaPhong: {id}");
                throw;
            }
        }

        public async Task<bool> DeletePhongHocAsync(int id)
        {
            try
            {
                var phong = await _context.PHONGHOC.FindAsync(id);
                if (phong == null) return false;

                // Để tránh xóa cứng gây mất dữ liệu liên quan, ta kiểm tra các ràng buộc
                var hasBookings = await _context.YEUCAUSUDUNGPHONG.AnyAsync(y => y.MaPhong == id);
                var hasIncidents = await _context.SUCO.AnyAsync(s => s.MaPhong == id);

                if (hasBookings || hasIncidents)
                {
                    // Chuyển trạng thái sang 'Ngừng sử dụng' thay vì xóa cứng
                    phong.TrangThai = "Ngừng sử dụng";
                    _context.PHONGHOC.Update(phong);
                    _logger.LogInformation($"Phòng {phong.TenPhong} đang liên kết dữ liệu mượn/sự cố, đổi trạng thái sang 'Ngừng sử dụng'.");
                }
                else
                {
                    // Xóa cứng
                    _context.PHONGHOC.Remove(phong);
                    _logger.LogInformation($"Xóa cứng phòng {phong.TenPhong} thành công.");
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong DeletePhongHocAsync cho MaPhong: {id}");
                throw;
            }
        }
    }
}
