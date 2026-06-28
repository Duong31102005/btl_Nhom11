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
    [Route("api/v1/thongke")]
    [Authorize(Roles = "Admin")]
    public class ApiThongKeController : ControllerBase
    {
        private readonly IThongKeService _thongKeService;
        private readonly ILogger<ApiThongKeController> _logger;

        public ApiThongKeController(IThongKeService thongKeService, ILogger<ApiThongKeController> logger)
        {
            _thongKeService = thongKeService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy nhật ký / lịch sử sử dụng phòng học (phân trang, lọc theo phòng, giảng viên, thời gian)
        /// </summary>
        [HttpGet("lichsu")]
        public async Task<IActionResult> GetLichSuDungPhong(
            [FromQuery] string? keyword,
            [FromQuery] int? maPhong,
            [FromQuery] int? maGiangVien,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (size < 1) size = 10;

                var result = await _thongKeService.GetLichSuDungPhongAsync(keyword, maPhong, maGiangVien, fromDate, toDate, page, size);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiThongKeController -> GetLichSuDungPhong");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tải lịch sử sử dụng phòng học."
                });
            }
        }

        /// <summary>
        /// Thống kê tần suất và số tiết sử dụng của từng phòng học
        /// </summary>
        [HttpGet("phong-tansuat")]
        public async Task<IActionResult> GetRoomUsageStats(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var result = await _thongKeService.GetRoomUsageStatsAsync(fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiThongKeController -> GetRoomUsageStats");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi thống kê tần suất sử dụng phòng học."
                });
            }
        }

        /// <summary>
        /// Thống kê sự cố của phòng học (tổng số sự cố, chưa xử lý, đã khắc phục)
        /// </summary>
        [HttpGet("suco-tansuat")]
        public async Task<IActionResult> GetIncidentStats()
        {
            try
            {
                var result = await _thongKeService.GetIncidentStatsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiThongKeController -> GetIncidentStats");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi thống kê tần suất sự cố phòng học."
                });
            }
        }

        /// <summary>
        /// Lấy tóm tắt các số liệu hiển thị trên Dashboard chính
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var result = await _thongKeService.GetDashboardSummaryAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiThongKeController -> GetDashboardSummary");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi lấy thông tin tổng hợp Dashboard."
                });
            }
        }
    }
}
