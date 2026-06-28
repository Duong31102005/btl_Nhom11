using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IThongKeService
    {
        Task<PagedResult<LichSuDungPhongDto>> GetLichSuDungPhongAsync(string? keyword, int? maPhong, int? maGiangVien, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize);
        Task<List<RoomUsageStatsDto>> GetRoomUsageStatsAsync(DateTime? fromDate, DateTime? toDate);
        Task<List<IncidentStatsDto>> GetIncidentStatsAsync();
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }

    public class ThongKeService : IThongKeService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<ThongKeService> _logger;

        public ThongKeService(QLPhongHocContext context, ILogger<ThongKeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<LichSuDungPhongDto>> GetLichSuDungPhongAsync(string? keyword, int? maPhong, int? maGiangVien, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.LICHSUDUNGPHONG
                    .AsNoTracking()
                    .Include(l => l.PhongHoc)
                    .Include(l => l.GiangVien)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(l => l.PhongHoc.TenPhong.ToLower().Contains(kw) || 
                                             l.GiangVien.HoTen.ToLower().Contains(kw) ||
                                             (l.NoiDung != null && l.NoiDung.ToLower().Contains(kw)));
                }

                if (maPhong.HasValue)
                {
                    query = query.Where(l => l.MaPhong == maPhong.Value);
                }

                if (maGiangVien.HasValue)
                {
                    query = query.Where(l => l.MaGiangVien == maGiangVien.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(l => l.NgaySuDung >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(l => l.NgaySuDung <= toDate.Value.Date);
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderByDescending(l => l.NgaySuDung)
                    .ThenByDescending(l => l.TietBatDau)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new LichSuDungPhongDto
                    {
                        MaLich = l.MaLich,
                        MaPhong = l.MaPhong,
                        TenPhong = l.PhongHoc != null ? l.PhongHoc.TenPhong : string.Empty,
                        MaGiangVien = l.MaGiangVien,
                        TenGiangVien = l.GiangVien != null ? l.GiangVien.HoTen : string.Empty,
                        MaYeuCau = l.MaYeuCau,
                        NgaySuDung = l.NgaySuDung,
                        TietBatDau = l.TietBatDau,
                        TietKetThuc = l.TietKetThuc,
                        NoiDung = l.NoiDung,
                        TrangThai = l.TrangThai
                    })
                    .ToListAsync();

                return new PagedResult<LichSuDungPhongDto>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetLichSuDungPhongAsync");
                throw;
            }
        }

        public async Task<List<RoomUsageStatsDto>> GetRoomUsageStatsAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.LICHSUDUNGPHONG.AsNoTracking().AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(l => l.NgaySuDung >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(l => l.NgaySuDung <= toDate.Value.Date);
                }

                // Nhóm theo phòng học và thống kê
                var stats = await query
                    .Where(l => l.TrangThai != "Đã hủy")
                    .GroupBy(l => new { l.MaPhong, l.PhongHoc.TenPhong })
                    .Select(g => new RoomUsageStatsDto
                    {
                        MaPhong = g.Key.MaPhong,
                        TenPhong = g.Key.TenPhong,
                        SoLanSuDung = g.Count(),
                        // Tính tổng số tiết mượn = SUM(TietKetThuc - TietBatDau + 1)
                        TongSoTiet = g.Sum(l => l.TietKetThuc - l.TietBatDau + 1)
                    })
                    .OrderByDescending(s => s.SoLanSuDung)
                    .ToListAsync();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetRoomUsageStatsAsync");
                throw;
            }
        }

        public async Task<List<IncidentStatsDto>> GetIncidentStatsAsync()
        {
            try
            {
                // Thống kê sự cố theo từng phòng học
                var stats = await _context.SUCO
                    .AsNoTracking()
                    .Include(s => s.PhongHoc)
                    .GroupBy(s => new { s.MaPhong, s.PhongHoc.TenPhong })
                    .Select(g => new IncidentStatsDto
                    {
                        MaPhong = g.Key.MaPhong,
                        TenPhong = g.Key.TenPhong,
                        SoVuSuCo = g.Count(),
                        SoVuChuaXuLy = g.Count(s => s.TrangThai == "Chờ xử lý" || s.TrangThai == "Đang xử lý"),
                        SoVuDaKhacPhuc = g.Count(s => s.TrangThai == "Đã khắc phục")
                    })
                    .OrderByDescending(s => s.SoVuSuCo)
                    .ToListAsync();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetIncidentStatsAsync");
                throw;
            }
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            try
            {
                var tongSoPhong = await _context.PHONGHOC.AsNoTracking().CountAsync();
                var tongSoThietBi = await _context.THIETBI.AsNoTracking().SumAsync(t => t.SoLuong);
                var soSuCoChuaXuLy = await _context.SUCO.AsNoTracking().CountAsync(s => s.TrangThai == "Chờ xử lý" || s.TrangThai == "Đang xử lý");
                var yeuCauChoDuyet = await _context.YEUCAUSUDUNGPHONG.AsNoTracking().CountAsync(y => y.TrangThai == "Chờ duyệt");

                return new DashboardSummaryDto
                {
                    TongSoPhong = tongSoPhong,
                    TongSoThietBi = tongSoThietBi,
                    SoSuCoChuaXuLy = soSuCoChuaXuLy,
                    YeuCauChoDuyet = yeuCauChoDuyet
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetDashboardSummaryAsync");
                throw;
            }
        }
    }
}
