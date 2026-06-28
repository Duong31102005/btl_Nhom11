using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Helpers;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers
{
    public class YeuCauSuDungPhongController : Controller
    {
        private readonly QLPhongHocContext _context;
        private readonly IDangKyService _dangKyService;

        public YeuCauSuDungPhongController(QLPhongHocContext context, IDangKyService dangKyService)
        {
            _context = context;
            _dangKyService = dangKyService;
        }

        public async Task<IActionResult> Index()
        {
            var tenVaiTro = HttpContext.Session.GetString(SessionHelper.SessionTenVaiTro);
            var userIdStr = HttpContext.Session.GetString(SessionHelper.SessionUserId);
            int.TryParse(userIdStr, out var userId);

            var query = _context.YEUCAUSUDUNGPHONG.Include(y => y.PhongHoc).Include(y => y.GiangVien).AsQueryable();

            if (tenVaiTro == "Giảng viên")
            {
                query = query.Where(y => y.MaGiangVien == userId);
            }

            var list = await query.OrderByDescending(y => y.NgayTao).ToListAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.Phongs = new SelectList(_context.PHONGHOC, "MaPhong", "TenPhong");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(YeuCauSuDungPhong model)
        {
            var userIdStr = HttpContext.Session.GetString(SessionHelper.SessionUserId);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            model.MaGiangVien = userId;
            model.NgayTao = DateTime.Now;

            // Check conflict
            var conflict = await _context.LICHSUDUNGPHONG.AnyAsync(l => l.MaPhong == model.MaPhong
                && l.NgaySuDung == model.NgaySuDung
                && l.TrangThai != "Đã hủy"
                && !(model.TietKetThuc < l.TietBatDau || model.TietBatDau > l.TietKetThuc));

            if (conflict)
            {
                TempData["Error"] = "Phòng đã có lịch sử dụng trong thời gian này.";
                ViewBag.Phongs = new SelectList(_context.PHONGHOC, "MaPhong", "TenPhong", model.MaPhong);
                return View(model);
            }

            model.TrangThai = "Chờ duyệt";
            _context.YEUCAUSUDUNGPHONG.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Tạo yêu cầu thành công. Chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var yc = await _context.YEUCAUSUDUNGPHONG.Include(y => y.PhongHoc).Include(y => y.GiangVien).FirstOrDefaultAsync(y => y.MaYeuCau == id);
            if (yc == null) return NotFound();
            return View(yc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var userIdStr = HttpContext.Session.GetString(SessionHelper.SessionUserId);
            int.TryParse(userIdStr, out var adminId);

            var req = new ApproveDangKyRequest { TrangThai = "Đã duyệt" };
            try
            {
                var success = await _dangKyService.ApproveDangKyAsync(id, req, adminId);
                if (success) TempData["Success"] = "Đã duyệt yêu cầu thành công.";
                else TempData["Error"] = "Không thể duyệt yêu cầu này.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string lyDoTuChoi)
        {
            var userIdStr = HttpContext.Session.GetString(SessionHelper.SessionUserId);
            int.TryParse(userIdStr, out var adminId);

            var req = new ApproveDangKyRequest { TrangThai = "Từ chối", LyDoTuChoi = lyDoTuChoi };
            try
            {
                var success = await _dangKyService.ApproveDangKyAsync(id, req, adminId);
                if (success) TempData["Success"] = "Đã từ chối yêu cầu thành công.";
                else TempData["Error"] = "Không thể từ chối yêu cầu này.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
