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
    [Route("api/v1/sucos")]
    [Authorize]
    public class ApiSuCoController : ControllerBase
    {
        private readonly ISuCoService _suCoService;
        private readonly ILogger<ApiSuCoController> _logger;

        public ApiSuCoController(ISuCoService suCoService, ILogger<ApiSuCoController> logger)
        {
            _suCoService = suCoService;
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

        /// <summary>
        /// Lấy danh sách sự cố (phân trang, lọc theo trạng thái, phòng học)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSuCos(
            [FromQuery] string? keyword,
            [FromQuery] string? trangThai,
            [FromQuery] int? maPhong,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (size < 1) size = 10;

                var result = await _suCoService.GetSuCosAsync(keyword, trangThai, maPhong, page, size);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiSuCoController -> GetSuCos");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tải danh sách sự cố."
                });
            }
        }

        /// <summary>
        /// Xem chi tiết sự cố theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSuCoById(int id)
        {
            try
            {
                var suCo = await _suCoService.GetSuCoByIdAsync(id);
                if (suCo == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "INCIDENT_NOT_FOUND",
                        Message = $"Không tìm thấy báo cáo sự cố với mã {id}"
                    });
                }
                return Ok(suCo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiSuCoController -> GetSuCoById ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi xem chi tiết sự cố."
                });
            }
        }

        /// <summary>
        /// Người dùng báo cáo sự cố phòng học hoặc thiết bị hỏng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSuCo([FromBody] CreateSuCoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Thông tin báo cáo sự cố không hợp lệ",
                        Details = ModelState
                    });
                }

                var userId = GetCurrentUserId();
                var created = await _suCoService.CreateSuCoAsync(request, userId);

                return CreatedAtAction(nameof(GetSuCoById), new { id = created.MaSuCo }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "INVALID_OPERATION",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiSuCoController -> CreateSuCo");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tạo báo cáo sự cố."
                });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái xử lý sự cố (Chỉ Admin)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSuCoStatus(int id, [FromBody] UpdateSuCoStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu cập nhật trạng thái sự cố không hợp lệ",
                        Details = ModelState
                    });
                }

                var adminId = GetCurrentUserId();
                var result = await _suCoService.UpdateSuCoStatusAsync(id, request, adminId);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "INCIDENT_NOT_FOUND",
                        Message = $"Không tìm thấy sự cố với mã {id}"
                    });
                }

                return Ok(new { Message = $"Đã cập nhật trạng thái sự cố mã {id} thành '{request.TrangThai}' thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiSuCoController -> UpdateSuCoStatus ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi cập nhật trạng thái sự cố."
                });
            }
        }

        /// <summary>
        /// Xóa báo cáo sự cố (Chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSuCo(int id)
        {
            try
            {
                var result = await _suCoService.DeleteSuCoAsync(id);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "INCIDENT_NOT_FOUND",
                        Message = $"Không tìm thấy báo cáo sự cố với mã {id} để xóa."
                    });
                }

                return Ok(new { Message = "Đã xóa báo cáo sự cố thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiSuCoController -> DeleteSuCo ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi xóa sự cố."
                });
            }
        }
    }
}
