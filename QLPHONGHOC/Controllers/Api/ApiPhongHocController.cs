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
    [Route("api/v1/phonghocs")]
    [Authorize]
    public class ApiPhongHocController : ControllerBase
    {
        private readonly IPhongHocService _phongHocService;
        private readonly ILogger<ApiPhongHocController> _logger;

        public ApiPhongHocController(IPhongHocService phongHocService, ILogger<ApiPhongHocController> logger)
        {
            _phongHocService = phongHocService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách phòng học (phân trang, lọc, tìm kiếm)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPhongHocs(
            [FromQuery] string? keyword,
            [FromQuery] string? loaiPhong,
            [FromQuery] string? trangThai,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (size < 1) size = 10;

                var result = await _phongHocService.GetPhongHocsAsync(keyword, loaiPhong, trangThai, page, size);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiPhongHocController -> GetPhongHocs");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tải danh sách phòng học."
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết phòng học theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPhongHocById(int id)
        {
            try
            {
                var phong = await _phongHocService.GetPhongHocByIdAsync(id);
                if (phong == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "PHONGHOC_NOT_FOUND",
                        Message = $"Không tìm thấy phòng học với mã {id}"
                    });
                }
                return Ok(phong);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiPhongHocController -> GetPhongHocById ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi lấy chi tiết phòng học."
                });
            }
        }

        /// <summary>
        /// Tạo mới phòng học (Yêu cầu quyền Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePhongHoc([FromBody] CreatePhongHocRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu tạo phòng không hợp lệ",
                        Details = ModelState
                    });
                }

                var created = await _phongHocService.CreatePhongHocAsync(request);
                return CreatedAtAction(nameof(GetPhongHocById), new { id = created.MaPhong }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "PHONGHOC_EXISTS",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiPhongHocController -> CreatePhongHoc");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tạo phòng học mới."
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin phòng học (Yêu cầu quyền Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePhongHoc(int id, [FromBody] UpdatePhongHocRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu cập nhật phòng không hợp lệ",
                        Details = ModelState
                    });
                }

                var updated = await _phongHocService.UpdatePhongHocAsync(id, request);
                if (updated == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "PHONGHOC_NOT_FOUND",
                        Message = $"Không tìm thấy phòng học với mã {id} để cập nhật"
                    });
                }

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "PHONGHOC_EXISTS",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiPhongHocController -> UpdatePhongHoc ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi cập nhật thông tin phòng học."
                });
            }
        }

        /// <summary>
        /// Xóa phòng học (Yêu cầu quyền Admin - Khóa mềm thành Ngừng sử dụng nếu đã có lịch mượn)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePhongHoc(int id)
        {
            try
            {
                var result = await _phongHocService.DeletePhongHocAsync(id);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "PHONGHOC_NOT_FOUND",
                        Message = $"Không tìm thấy phòng học với mã {id} để xóa"
                    });
                }

                return Ok(new { Message = "Đã thực hiện xóa hoặc cập nhật ngưng hoạt động phòng học thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiPhongHocController -> DeletePhongHoc ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi xóa phòng học."
                });
            }
        }
    }
}
