using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Models;
using QLPhongHoc.Models.Auth;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers.Api
{
    [ApiController]
    [Route("api/v1/dangky")]
    [Authorize]
    public class ApiDangKyController : ControllerBase
    {
        private readonly IDangKyService _dangKyService;
        private readonly ILogger<ApiDangKyController> _logger;

        public ApiDangKyController(IDangKyService dangKyService, ILogger<ApiDangKyController> logger)
        {
            _dangKyService = dangKyService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("Không xác định được ID người dùng từ Token.");
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst("VaiTro")?.Value ?? "User";
        }

        /// <summary>
        /// Lấy danh sách yêu cầu đăng ký (phân trang, lọc).
        /// Giảng viên chỉ xem được của chính mình. Admin xem được toàn bộ.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDangKys(
            [FromQuery] string? keyword,
            [FromQuery] string? trangThai,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (size < 1) size = 10;

                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();

                var result = await _dangKyService.GetDangKysAsync(keyword, trangThai, fromDate, toDate, page, size, userId, role);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiDangKyController -> GetDangKys");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tải danh sách đăng ký."
                });
            }
        }

        /// <summary>
        /// Xem chi tiết một yêu cầu đăng ký
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDangKyById(int id)
        {
            try
            {
                var result = await _dangKyService.GetDangKyByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "BOOKING_NOT_FOUND",
                        Message = $"Không tìm thấy yêu cầu đặt phòng với mã {id}"
                    });
                }

                // Bảo mật dữ liệu: Giảng viên không được xem chi tiết yêu cầu của người khác
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();
                if (role == "Giảng viên" && result.MaGiangVien != userId)
                {
                    return Forbid();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiDangKyController -> GetDangKyById ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi xem chi tiết yêu cầu đăng ký."
                });
            }
        }

        /// <summary>
        /// Tạo mới một yêu cầu đặt phòng (Giảng viên gửi yêu cầu)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDangKy([FromBody] CreateDangKyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu đăng ký phòng không hợp lệ",
                        Details = ModelState
                    });
                }

                var userId = GetCurrentUserId();
                var created = await _dangKyService.CreateDangKyAsync(request, userId);

                return CreatedAtAction(nameof(GetDangKyById), new { id = created.MaYeuCau }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "BOOKING_CONFLICT",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiDangKyController -> CreateDangKy");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tạo yêu cầu đăng ký."
                });
            }
        }

        /// <summary>
        /// Phê duyệt yêu cầu đặt phòng (Chỉ Admin)
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveDangKy(int id, [FromBody] ApproveDangKyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu duyệt không hợp lệ",
                        Details = ModelState
                    });
                }

                var adminId = GetCurrentUserId();
                var result = await _dangKyService.ApproveDangKyAsync(id, request, adminId);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "BOOKING_NOT_FOUND",
                        Message = $"Không tìm thấy yêu cầu đặt phòng hoặc yêu cầu không thể duyệt."
                    });
                }

                return Ok(new { Message = $"Đã xử lý phê duyệt yêu cầu đặt phòng thành công: {request.TrangThai}" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "BOOKING_CONFLICT_OR_INVALID",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiDangKyController -> ApproveDangKy ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi duyệt yêu cầu đăng ký."
                });
            }
        }

        /// <summary>
        /// Hủy yêu cầu đặt phòng (Giảng viên hủy của họ hoặc Admin hủy)
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelDangKy(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();

                var result = await _dangKyService.CancelDangKyAsync(id, userId, role);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "BOOKING_NOT_FOUND",
                        Message = $"Không tìm thấy yêu cầu đặt phòng cần hủy."
                    });
                }

                return Ok(new { Message = "Hủy yêu cầu đặt phòng thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "INVALID_OPERATION",
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiErrorResponse
                {
                    ErrorCode = "FORBIDDEN",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiDangKyController -> CancelDangKy ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi hủy đăng ký."
                });
            }
        }
    }
}
