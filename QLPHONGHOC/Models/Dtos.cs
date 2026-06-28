using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLPhongHoc.Models
{
    // ==========================================
    // PAGINATION HELPER
    // ==========================================
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    // ==========================================
    // MODULE 1: ACCOUNT DTOS
    // ==========================================
    public class AccountDto
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
    }

    public class UpdateAccountRequest
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int MaVaiTro { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(30, ErrorMessage = "Trạng thái tối đa 30 ký tự")]
        public string TrangThai { get; set; } = string.Empty; // 'Hoạt động', 'Khóa', etc.
    }

    public class ApproveAccountRequest
    {
        [Required(ErrorMessage = "Trạng thái duyệt là bắt buộc")]
        [RegularExpression("^(Hoạt động|Từ chối)$", ErrorMessage = "Trạng thái duyệt phải là 'Hoạt động' hoặc 'Từ chối'")]
        public string TrangThai { get; set; } = string.Empty;
    }

    // ==========================================
    // MODULE 2: PHONGHOC DTOS
    // ==========================================
    public class PhongHocDto
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public string DayNha { get; set; } = string.Empty;
        public int Tang { get; set; }
        public int SucChua { get; set; }
        public string LoaiPhong { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class CreatePhongHocRequest
    {
        [Required(ErrorMessage = "Tên phòng là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên phòng tối đa 50 ký tự")]
        public string TenPhong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dãy nhà là bắt buộc")]
        [StringLength(50, ErrorMessage = "Dãy nhà tối đa 50 ký tự")]
        public string DayNha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tầng là bắt buộc")]
        [Range(0, 50, ErrorMessage = "Tầng phải nằm trong khoảng 0 đến 50")]
        public int Tang { get; set; }

        [Required(ErrorMessage = "Sức chứa là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Sức chứa phải lớn hơn 0")]
        public int SucChua { get; set; }

        [Required(ErrorMessage = "Loại phòng là bắt buộc")]
        [StringLength(50, ErrorMessage = "Loại phòng tối đa 50 ký tự")]
        public string LoaiPhong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(50, ErrorMessage = "Trạng thái tối đa 50 ký tự")]
        public string TrangThai { get; set; } = string.Empty; // 'Hoạt động', 'Cần sửa chữa', 'Trống', 'Bận'

        [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự")]
        public string? GhiChu { get; set; }
    }

    public class UpdatePhongHocRequest : CreatePhongHocRequest
    {
    }

    // ==========================================
    // MODULE 3: THIETBI DTOS
    // ==========================================
    public class ThietBiDto
    {
        public int MaThietBi { get; set; }
        public string TenThietBi { get; set; } = string.Empty;
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public string TinhTrang { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class CreateThietBiRequest
    {
        [Required(ErrorMessage = "Tên thiết bị là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên thiết bị tối đa 100 ký tự")]
        public string TenThietBi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phòng thuộc về là bắt buộc")]
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Tình trạng là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tình trạng tối đa 50 ký tự")]
        public string TinhTrang { get; set; } = string.Empty; // 'Hoạt động', 'Cần sửa chữa', 'Hỏng', 'Ngừng sử dụng'

        [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự")]
        public string? GhiChu { get; set; }
    }

    public class UpdateThietBiRequest : CreateThietBiRequest
    {
    }

    // ==========================================
    // MODULE 4: DANGKY DTOS
    // ==========================================
    public class DangKyDto
    {
        public int MaYeuCau { get; set; }
        public int MaGiangVien { get; set; }
        public string TenGiangVien { get; set; } = string.Empty;
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public DateTime NgaySuDung { get; set; }
        public int TietBatDau { get; set; }
        public int TietKetThuc { get; set; }
        public string? MucDich { get; set; }
        public string TrangThai { get; set; } = string.Empty; // 'Chờ duyệt', 'Đã duyệt', 'Từ chối', 'Đã hủy'
        public string? LyDoTuChoi { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class CreateDangKyRequest
    {
        [Required(ErrorMessage = "Phòng học là bắt buộc")]
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Ngày sử dụng là bắt buộc")]
        public DateTime NgaySuDung { get; set; }

        [Required(ErrorMessage = "Tiết bắt đầu là bắt buộc")]
        [Range(1, 15, ErrorMessage = "Tiết bắt đầu phải từ 1 đến 15")]
        public int TietBatDau { get; set; }

        [Required(ErrorMessage = "Tiết kết thúc là bắt buộc")]
        [Range(1, 15, ErrorMessage = "Tiết kết thúc phải từ 1 đến 15")]
        public int TietKetThuc { get; set; }

        [StringLength(255, ErrorMessage = "Mục đích mượn tối đa 255 ký tự")]
        public string? MucDich { get; set; }
    }

    public class ApproveDangKyRequest
    {
        [Required(ErrorMessage = "Quyết định phê duyệt là bắt buộc")]
        [RegularExpression("^(Đã duyệt|Từ chối)$", ErrorMessage = "Chỉ có thể chọn 'Đã duyệt' hoặc 'Từ chối'")]
        public string TrangThai { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Lý do từ chối tối đa 255 ký tự")]
        public string? LyDoTuChoi { get; set; }
    }

    // ==========================================
    // MODULE 5: SUCO DTOS
    // ==========================================
    public class SuCoDto
    {
        public int MaSuCo { get; set; }
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public int? MaThietBi { get; set; }
        public string? TenThietBi { get; set; }
        public int NguoiBaoCao { get; set; }
        public string TenNguoiBaoCao { get; set; } = string.Empty;
        public string MoTaSuCo { get; set; } = string.Empty;
        public DateTime NgayBaoCao { get; set; }
        public string TrangThai { get; set; } = string.Empty; // 'Chờ xử lý', 'Đang xử lý', 'Đã khắc phục', 'Không khắc phục được'
    }

    public class CreateSuCoRequest
    {
        [Required(ErrorMessage = "Mã phòng học là bắt buộc")]
        public int MaPhong { get; set; }

        public int? MaThietBi { get; set; }

        [Required(ErrorMessage = "Mô tả sự cố là bắt buộc")]
        [StringLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
        public string MoTaSuCo { get; set; } = string.Empty;
    }

    public class UpdateSuCoStatusRequest
    {
        [Required(ErrorMessage = "Trạng thái sự cố là bắt buộc")]
        [RegularExpression("^(Chờ xử lý|Đang xử lý|Đã khắc phục|Không khắc phục được)$", ErrorMessage = "Trạng thái không hợp lệ")]
        public string TrangThai { get; set; } = string.Empty;
    }

    // ==========================================
    // MODULE 6: BAOTRI DTOS
    // ==========================================
    public class BaoTriDto
    {
        public int MaBaoTri { get; set; }
        public int MaSuCo { get; set; }
        public string MoTaSuCo { get; set; } = string.Empty;
        public string TenPhong { get; set; } = string.Empty;
        public string? TenThietBi { get; set; }
        public int MaKyThuatVien { get; set; }
        public string TenKyThuatVien { get; set; } = string.Empty;
        public DateTime NgayXuLy { get; set; }
        public string NoiDungXuLy { get; set; } = string.Empty;
        public string KetQua { get; set; } = string.Empty; // 'Thành công', 'Thất bại', 'Đang sửa'
    }

    public class CreateBaoTriRequest
    {
        [Required(ErrorMessage = "Mã sự cố liên quan là bắt buộc")]
        public int MaSuCo { get; set; }

        [Required(ErrorMessage = "Mã kỹ thuật viên phụ trách là bắt buộc")]
        public int MaKyThuatVien { get; set; }

        [Required(ErrorMessage = "Nội dung xử lý ban đầu là bắt buộc")]
        [StringLength(255, ErrorMessage = "Nội dung tối đa 255 ký tự")]
        public string NoiDungXuLy { get; set; } = string.Empty;
    }

    public class UpdateBaoTriRequest
    {
        [Required(ErrorMessage = "Nội dung xử lý/cập nhật là bắt buộc")]
        [StringLength(255, ErrorMessage = "Nội dung tối đa 255 ký tự")]
        public string NoiDungXuLy { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kết quả bảo trì là bắt buộc")]
        [RegularExpression("^(Thành công|Thất bại|Đang sửa)$", ErrorMessage = "Kết quả không hợp lệ")]
        public string KetQua { get; set; } = string.Empty;
    }

    // ==========================================
    // MODULE 7: THONGKE DTOS
    // ==========================================
    public class LichSuDungPhongDto
    {
        public int MaLich { get; set; }
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public int MaGiangVien { get; set; }
        public string TenGiangVien { get; set; } = string.Empty;
        public int? MaYeuCau { get; set; }
        public DateTime NgaySuDung { get; set; }
        public int TietBatDau { get; set; }
        public int TietKetThuc { get; set; }
        public string? NoiDung { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class RoomUsageStatsDto
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public int SoLanSuDung { get; set; }
        public int TongSoTiet { get; set; }
    }

    public class IncidentStatsDto
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public int SoVuSuCo { get; set; }
        public int SoVuChuaXuLy { get; set; }
        public int SoVuDaKhacPhuc { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int TongSoPhong { get; set; }
        public int TongSoThietBi { get; set; }
        public int SoSuCoChuaXuLy { get; set; }
        public int YeuCauChoDuyet { get; set; }
    }
}
