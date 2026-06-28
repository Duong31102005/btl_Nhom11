using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IDangKyService
    {
        Task<PagedResult<DangKyDto>> GetDangKysAsync(string? keyword, string? trangThai, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, int? currentUserId = null, string? currentUserRole = null);
        Task<DangKyDto?> GetDangKyByIdAsync(int id);
        Task<DangKyDto> CreateDangKyAsync(CreateDangKyRequest request, int userId);
        Task<bool> ApproveDangKyAsync(int id, ApproveDangKyRequest request, int adminId);
        Task<bool> CancelDangKyAsync(int id, int userId, string userRole);
    }

    public class DangKyService : IDangKyService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<DangKyService> _logger;

        public DangKyService(QLPhongHocContext context, ILogger<DangKyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<DangKyDto>> GetDangKysAsync(string? keyword, string? trangThai, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, int? currentUserId = null, string? currentUserRole = null)
        {
            try
            {
                var query = _context.YEUCAUSUDUNGPHONG
                    .AsNoTracking()
                    .Include(y => y.PhongHoc)
                    .Include(y => y.GiangVien)
                    .AsQueryable();

                // Phân quyền dữ liệu: Giảng viên chỉ xem được yêu cầu của mình
                if (currentUserRole == "Giảng viên" && currentUserId.HasValue)
                {
                    query = query.Where(y => y.MaGiangVien == currentUserId.Value);
                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(y => y.PhongHoc.TenPhong.ToLower().Contains(kw) || 
                                             y.GiangVien.HoTen.ToLower().Contains(kw));
                }

                if (!string.IsNullOrWhiteSpace(trangThai))
                {
                    query = query.Where(y => y.TrangThai == trangThai);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(y => y.NgaySuDung >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(y => y.NgaySuDung <= toDate.Value.Date.AddDays(1).AddTicks(-1));
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderByDescending(y => y.NgayTao)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(y => new DangKyDto
                    {
                        MaYeuCau = y.MaYeuCau,
                        MaGiangVien = y.MaGiangVien,
                        TenGiangVien = y.GiangVien != null ? y.GiangVien.HoTen : string.Empty,
                        MaPhong = y.MaPhong,
                        TenPhong = y.PhongHoc != null ? y.PhongHoc.TenPhong : string.Empty,
                        NgaySuDung = y.NgaySuDung,
                        TietBatDau = y.TietBatDau,
                        TietKetThuc = y.TietKetThuc,
                        MucDich = y.MucDich,
                        TrangThai = y.TrangThai,
                        LyDoTuChoi = y.LyDoTuChoi,
                        NgayTao = y.NgayTao
                    })
                    .ToListAsync();

                return new PagedResult<DangKyDto>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetDangKysAsync");
                throw;
            }
        }

        public async Task<DangKyDto?> GetDangKyByIdAsync(int id)
        {
            try
            {
                var y = await _context.YEUCAUSUDUNGPHONG
                    .AsNoTracking()
                    .Include(x => x.PhongHoc)
                    .Include(x => x.GiangVien)
                    .FirstOrDefaultAsync(x => x.MaYeuCau == id);

                if (y == null) return null;

                return new DangKyDto
                {
                    MaYeuCau = y.MaYeuCau,
                    MaGiangVien = y.MaGiangVien,
                    TenGiangVien = y.GiangVien != null ? y.GiangVien.HoTen : string.Empty,
                    MaPhong = y.MaPhong,
                    TenPhong = y.PhongHoc != null ? y.PhongHoc.TenPhong : string.Empty,
                    NgaySuDung = y.NgaySuDung,
                    TietBatDau = y.TietBatDau,
                    TietKetThuc = y.TietKetThuc,
                    MucDich = y.MucDich,
                    TrangThai = y.TrangThai,
                    LyDoTuChoi = y.LyDoTuChoi,
                    NgayTao = y.NgayTao
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong GetDangKyByIdAsync cho MaYeuCau: {id}");
                throw;
            }
        }

        public async Task<DangKyDto> CreateDangKyAsync(CreateDangKyRequest request, int userId)
        {
            try
            {
                if (request.TietBatDau > request.TietKetThuc)
                {
                    throw new InvalidOperationException("Tiết bắt đầu không thể lớn hơn tiết kết thúc.");
                }

                // Kiểm tra phòng học tồn tại và hoạt động
                var phong = await _context.PHONGHOC.FindAsync(request.MaPhong);
                if (phong == null)
                {
                    throw new InvalidOperationException("Phòng học không tồn tại.");
                }
                if (phong.TrangThai == "Ngừng sử dụng")
                {
                    throw new InvalidOperationException("Phòng học đang ngừng hoạt động, không thể đăng ký.");
                }

                // Kiểm tra xem giảng viên có tồn tại không
                var gv = await _context.TAIKHOAN.FindAsync(userId);
                if (gv == null)
                {
                    throw new InvalidOperationException("Giảng viên không tồn tại.");
                }

                // KIỂM TRA TRÙNG LỊCH: Kiểm tra xem trong bảng LICHSUDUNGPHONG (lịch sử và lịch sử dụng chính thức) có lịch nào đang hoạt động và trùng thời gian không
                var isConflict = await _context.LICHSUDUNGPHONG.AnyAsync(l => 
                    l.MaPhong == request.MaPhong &&
                    l.NgaySuDung.Date == request.NgaySuDung.Date &&
                    l.TrangThai != "Đã hủy" &&
                    !(request.TietKetThuc < l.TietBatDau || request.TietBatDau > l.TietKetThuc));

                if (isConflict)
                {
                    throw new InvalidOperationException("Phòng học đã được đăng ký hoặc đang sử dụng trong khoảng thời gian này.");
                }

                // Kiểm tra xem có yêu cầu "Đã duyệt" nào khác trong bảng DANGKY trùng lịch chưa đồng bộ không
                var isConflictInRequests = await _context.YEUCAUSUDUNGPHONG.AnyAsync(y => 
                    y.MaPhong == request.MaPhong &&
                    y.NgaySuDung.Date == request.NgaySuDung.Date &&
                    y.TrangThai == "Đã duyệt" &&
                    !(request.TietKetThuc < y.TietBatDau || request.TietBatDau > y.TietKetThuc));

                if (isConflictInRequests)
                {
                    throw new InvalidOperationException("Phòng học đã được phê duyệt cho một yêu cầu khác trong khoảng thời gian này.");
                }

                var yeuCau = new YeuCauSuDungPhong
                {
                    MaGiangVien = userId,
                    MaPhong = request.MaPhong,
                    NgaySuDung = request.NgaySuDung.Date,
                    TietBatDau = request.TietBatDau,
                    TietKetThuc = request.TietKetThuc,
                    MucDich = request.MucDich,
                    TrangThai = "Chờ duyệt",
                    NgayTao = DateTime.Now
                };

                _context.YEUCAUSUDUNGPHONG.Add(yeuCau);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Giảng viên {gv.HoTen} đã tạo yêu cầu mượn phòng {phong.TenPhong} vào ngày {request.NgaySuDung:dd/MM/yyyy}");

                return await GetDangKyByIdAsync(yeuCau.MaYeuCau) ?? new DangKyDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong CreateDangKyAsync");
                throw;
            }
        }

        public async Task<bool> ApproveDangKyAsync(int id, ApproveDangKyRequest request, int adminId)
        {
            try
            {
                var yc = await _context.YEUCAUSUDUNGPHONG
                    .Include(y => y.PhongHoc)
                    .Include(y => y.GiangVien)
                    .FirstOrDefaultAsync(y => y.MaYeuCau == id);

                if (yc == null) return false;

                if (yc.TrangThai != "Chờ duyệt")
                {
                    throw new InvalidOperationException("Chỉ có thể duyệt các yêu cầu ở trạng thái 'Chờ duyệt'.");
                }

                if (request.TrangThai == "Đã duyệt")
                {
                    // Kiểm tra lại trùng lịch trước khi phê duyệt chính thức (Tránh Race Condition)
                    var isConflict = await _context.LICHSUDUNGPHONG.AnyAsync(l => 
                        l.MaPhong == yc.MaPhong &&
                        l.NgaySuDung.Date == yc.NgaySuDung.Date &&
                        l.TrangThai != "Đã hủy" &&
                        !(yc.TietKetThuc < l.TietBatDau || yc.TietBatDau > l.TietKetThuc));

                    if (isConflict)
                    {
                        throw new InvalidOperationException("Không thể duyệt vì phòng học đã có lịch sử dụng trùng khớp phát sinh.");
                    }

                    yc.TrangThai = "Đã duyệt";

                    // Thêm bản ghi vào bảng LICHSUDUNGPHONG để làm lịch mượn chính thức
                    var lichSuDung = new LichSuDungPhong
                    {
                        MaPhong = yc.MaPhong,
                        MaGiangVien = yc.MaGiangVien,
                        MaYeuCau = yc.MaYeuCau,
                        NgaySuDung = yc.NgaySuDung,
                        TietBatDau = yc.TietBatDau,
                        TietKetThuc = yc.TietKetThuc,
                        NoiDung = yc.MucDich,
                        TrangThai = "Đã duyệt"
                    };
                    _context.LICHSUDUNGPHONG.Add(lichSuDung);

                    // Ghi log vào bảng LICHSU
                    var log = new LichSu
                    {
                        LoaiLichSu = "DangKy",
                        NoiDung = $"Phê duyệt yêu cầu đặt phòng {yc.PhongHoc.TenPhong} cho giảng viên {yc.GiangVien.HoTen} vào ngày {yc.NgaySuDung:dd/MM/yyyy} (Tiết {yc.TietBatDau}-{yc.TietKetThuc})",
                        NguoiThucHien = adminId
                    };
                    _context.LICHSU.Add(log);
                }
                else if (request.TrangThai == "Từ chối")
                {
                    yc.TrangThai = "Từ chối";
                    yc.LyDoTuChoi = request.LyDoTuChoi;

                    // Ghi log từ chối vào bảng LICHSU
                    var log = new LichSu
                    {
                        LoaiLichSu = "DangKy",
                        NoiDung = $"Từ chối yêu cầu đặt phòng {yc.PhongHoc.TenPhong} của giảng viên {yc.GiangVien.HoTen} vào ngày {yc.NgaySuDung:dd/MM/yyyy}. Lý do: {request.LyDoTuChoi}",
                        NguoiThucHien = adminId
                    };
                    _context.LICHSU.Add(log);
                }

                _context.YEUCAUSUDUNGPHONG.Update(yc);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin {adminId} xử lý yêu cầu mượn phòng {id} thành: {request.TrangThai}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApproveDangKyAsync cho MaYeuCau: {id}");
                throw;
            }
        }

        public async Task<bool> CancelDangKyAsync(int id, int userId, string userRole)
        {
            try
            {
                var yc = await _context.YEUCAUSUDUNGPHONG.FindAsync(id);
                if (yc == null) return false;

                // Giảng viên chỉ được hủy yêu cầu của chính họ
                if (userRole == "Giảng viên" && yc.MaGiangVien != userId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền hủy yêu cầu đặt phòng của người khác.");
                }

                if (yc.TrangThai == "Đã hủy" || yc.TrangThai == "Từ chối")
                {
                    throw new InvalidOperationException("Yêu cầu đã ở trạng thái hủy hoặc bị từ chối.");
                }

                yc.TrangThai = "Đã hủy";
                _context.YEUCAUSUDUNGPHONG.Update(yc);

                // Nếu yêu cầu đã từng được duyệt, cần hủy bản ghi trong LICHSUDUNGPHONG tương ứng
                var lichSuDung = await _context.LICHSUDUNGPHONG.FirstOrDefaultAsync(l => l.MaYeuCau == id);
                if (lichSuDung != null)
                {
                    lichSuDung.TrangThai = "Đã hủy";
                    _context.LICHSUDUNGPHONG.Update(lichSuDung);
                }

                // Ghi log hủy vào bảng LICHSU
                var log = new LichSu
                {
                    LoaiLichSu = "DangKy",
                    NoiDung = $"Hủy yêu cầu đặt phòng mã {id} bởi {(userRole == "Admin" ? "Admin" : "Giảng viên")}",
                    NguoiThucHien = userId
                };
                _context.LICHSU.Add(log);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Hủy yêu cầu mượn phòng {id} bởi người dùng {userId} thành công.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong CancelDangKyAsync cho MaYeuCau: {id}");
                throw;
            }
        }
    }
}
