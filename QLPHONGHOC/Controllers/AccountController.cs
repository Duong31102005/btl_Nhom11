using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Helpers;
using QLPhongHoc.Models;
using QLPhongHoc.Models.Auth;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers
{
    public class AccountController : Controller
    {
        private readonly QLPhongHocContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            QLPhongHocContext context,
            IAuthService authService,
            ILogger<AccountController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Account/Login - Hiển thị trang đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/Login - Xử lý đăng nhập
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string TenDangNhap, string MatKhau, bool GhiNho = false)
        {
            var loginRequest = new LoginRequest
            {
                TenDangNhap = TenDangNhap,
                MatKhau = MatKhau,
                GhiNho = GhiNho
            };

            var response = await _authService.LoginAsync(loginRequest);

            if (!response.Success)
            {
                ViewBag.Error = response.Message;
                return View();
            }

            // Lưu vào Session
            Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                HttpContext.Session,
                SessionHelper.SessionUserId,
                response.UserInfo.MaTaiKhoan.ToString());

            Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                HttpContext.Session,
                SessionHelper.SessionHoTen,
                response.UserInfo.HoTen ?? string.Empty);

            Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                HttpContext.Session,
                SessionHelper.SessionTenVaiTro,
                response.UserInfo.TenVaiTro ?? string.Empty);

            Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                HttpContext.Session,
                SessionHelper.SessionMaVaiTro,
                response.UserInfo.MaVaiTro.ToString());

            // Lưu JWT Token vào Session (nếu cần)
            Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                HttpContext.Session,
                "JwtToken",
                response.Token);

            _logger.LogInformation($"User {response.UserInfo.TenDangNhap} logged in successfully");
            TempData["Success"] = "Đăng nhập thành công.";

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// POST: /api/Account/Login - API endpoint cho đăng nhập (hỗ trợ JSON)
        /// </summary>
        [HttpPost]
        [Route("api/Account/Login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "INVALID_INPUT",
                    Message = "Dữ liệu đầu vào không hợp lệ",
                    Details = ModelState
                });
            }

            var response = await _authService.LoginAsync(request);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// GET: /Account/Logout - Đăng xuất
        /// </summary>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// GET: /Account/Register - Hiển thị trang đăng ký
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/Register - Xử lý đăng ký
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
            {
                ViewBag.Error = response.Message;
                return View();
            }

            _logger.LogInformation($"User {request.TenDangNhap} registered successfully");
            TempData["Success"] = response.Message;

            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// POST: /api/Account/Register - API endpoint cho đăng ký
        /// </summary>
        [HttpPost]
        [Route("api/Account/Register")]
        public async Task<IActionResult> ApiRegister([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "INVALID_INPUT",
                    Message = "Dữ liệu đầu vào không hợp lệ",
                    Details = ModelState
                });
            }

            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
