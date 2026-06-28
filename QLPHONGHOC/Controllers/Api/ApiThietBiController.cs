using System;
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
    [Route("api/v1/thietbis")]
    [Authorize]
    public class ApiThietBiController : ControllerBase
    {
        private readonly IThietBiService _thietBiService;
        private readonly ILogger<ApiThietBiController> _logger;

        public ApiThietBiController(IThietBiService thietBiService, ILogger<ApiThietBiController> logger)
        {
            _thietBiService = thietBiService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách thiết bị (phân trang, lọc theo tình trạng, phòng)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetThietBis(
            [FromQuery] string? keyword,
            [FromQuery] int? maPhong,
            [FromQuery] string? tinhTrang,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (size < 1) size = 10;

                var result = await _thietBiService.GetThietBisAsync(keyword, maPhong, tinhTrang, page, size);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiThietBiController -> GetThietBis");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tải danh sách thiết bị."
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết thiết bị theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetThietBiById(int id)
        {
            try
            {
                var tb = await _thietBiService.GetThietBiByIdAsync(id);
                if (tb == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "THIETBI_NOT_FOUND",
                        Message = $"Không tìm thấy thiết bị với mã {id}"
                    });
                }
                return Ok(tb);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiThietBiController -> GetThietBiById ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi lấy chi tiết thiết bị."
                });
            }
        }

        /// <summary>
        /// Tạo mới thiết bị (Yêu cầu quyền Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateThietBi([FromBody] CreateThietBiRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu tạo thiết bị không hợp lệ",
                        Details = ModelState
                    });
                }

                var created = await _thietBiService.CreateThietBiAsync(request);
                return CreatedAtAction(nameof(GetThietBiById), new { id = created.MaThietBi }, created);
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
                _logger.LogError(ex, "Lỗi xảy ra trong ApiThietBiController -> CreateThietBi");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi thêm thiết bị mới."
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin thiết bị (Yêu cầu quyền Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateThietBi(int id, [FromBody] UpdateThietBiRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu cập nhật thiết bị không hợp lệ",
                        Details = ModelState
                    });
                }

                var updated = await _thietBiService.UpdateThietBiAsync(id, request);
                if (updated == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "THIETBI_NOT_FOUND",
                        Message = $"Không tìm thấy thiết bị với mã {id} để cập nhật"
                    });
                }

                return Ok(updated);
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
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiThietBiController -> UpdateThietBi ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi cập nhật thiết bị."
                });
            }
        }

        /// <summary>
        /// Xóa thiết bị (Yêu cầu quyền Admin - Đổi thành Ngừng sử dụng nếu thiết bị gắn liền với sự cố)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteThietBi(int id)
        {
            try
            {
                var result = await _thietBiService.DeleteThietBiAsync(id);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "THIETBI_NOT_FOUND",
                        Message = $"Không tìm thấy thiết bị với mã {id} để xóa"
                    });
                }

                return Ok(new { Message = "Đã thực hiện xóa thiết bị hoặc đổi trạng thái thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiThietBiController -> DeleteThietBi ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi xóa thiết bị."
                });
            }
        }
    }
}
