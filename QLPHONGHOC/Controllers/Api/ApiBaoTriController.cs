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
    [Route("api/v1/baotris")]
    [Authorize(Roles = "Admin,Kỹ thuật viên")]
    public class ApiBaoTriController : ControllerBase
    {
        private readonly IBaoTriService _baoTriService;
        private readonly ILogger<ApiBaoTriController> _logger;

        public ApiBaoTriController(IBaoTriService baoTriService, ILogger<ApiBaoTriController> logger)
        {
            _baoTriService = baoTriService;
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
        /// Lấy danh sách phiếu bảo trì (phân trang, lọc theo kết quả, kỹ thuật viên phụ trách)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBaoTris(
            [FromQuery] string? keyword,
            [FromQuery] string? ketQua,
            [FromQuery] int? maKyThuatVien,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (size < 1) size = 10;

                var result = await _baoTriService.GetBaoTrisAsync(keyword, ketQua, maKyThuatVien, page, size);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiBaoTriController -> GetBaoTris");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tải danh sách bảo trì."
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết phiếu bảo trì theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBaoTriById(int id)
        {
            try
            {
                var baoTri = await _baoTriService.GetBaoTriByIdAsync(id);
                if (baoTri == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "MAINTENANCE_NOT_FOUND",
                        Message = $"Không tìm thấy phiếu bảo trì với mã {id}"
                    });
                }
                return Ok(baoTri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiBaoTriController -> GetBaoTriById ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi xem chi tiết phiếu bảo trì."
                });
            }
        }

        /// <summary>
        /// Tạo phiếu bảo trì / lên lịch sửa chữa (Chỉ Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBaoTri([FromBody] CreateBaoTriRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu lập lịch sửa chữa không hợp lệ",
                        Details = ModelState
                    });
                }

                var created = await _baoTriService.CreateBaoTriAsync(request);
                return CreatedAtAction(nameof(GetBaoTriById), new { id = created.MaBaoTri }, created);
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
                _logger.LogError(ex, "Lỗi xảy ra trong ApiBaoTriController -> CreateBaoTri");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tạo phiếu bảo trì mới."
                });
            }
        }

        /// <summary>
        /// Cập nhật kết quả bảo trì (Kỹ thuật viên phụ trách hoặc Admin)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBaoTri(int id, [FromBody] UpdateBaoTriRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu cập nhật kết quả không hợp lệ",
                        Details = ModelState
                    });
                }

                var currentUserId = GetCurrentUserId();
                var updated = await _baoTriService.UpdateBaoTriAsync(id, request, currentUserId);
                if (updated == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "MAINTENANCE_NOT_FOUND",
                        Message = $"Không tìm thấy phiếu bảo trì với mã {id} để cập nhật"
                    });
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiBaoTriController -> UpdateBaoTri ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi cập nhật kết quả bảo trì."
                });
            }
        }

        /// <summary>
        /// Xóa phiếu bảo trì (Chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBaoTri(int id)
        {
            try
            {
                var result = await _baoTriService.DeleteBaoTriAsync(id);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "MAINTENANCE_NOT_FOUND",
                        Message = $"Không tìm thấy phiếu bảo trì với mã {id} để xóa"
                    });
                }

                return Ok(new { Message = "Đã xóa phiếu bảo trì thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiBaoTriController -> DeleteBaoTri ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi xóa phiếu bảo trì."
                });
            }
        }
    }
}
