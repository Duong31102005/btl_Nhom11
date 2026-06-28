using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Controllers
{
    [Authorize]
    public class PhongHocController : Controller
    {
        private readonly QLPhongHocContext _context;
        public PhongHocController(QLPhongHocContext context)
        {
            _context = context;
        }

        // GET: PhongHoc
        public async Task<IActionResult> Index(string searchName, string dayNha, string loaiPhong, string trangThai)
        {
            var query = _context.PHONGHOC.Include(p => p.LichSuDungPhongs).AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(p => p.TenPhong.Contains(searchName) || p.MaPhong.ToString() == searchName);
            if (!string.IsNullOrEmpty(dayNha))
                query = query.Where(p => p.DayNha.Contains(dayNha));
            if (!string.IsNullOrEmpty(loaiPhong))
                query = query.Where(p => p.LoaiPhong.Contains(loaiPhong));
            
            // To filter by TrangThai accurately, we first fetch the list and calculate real-time status
            var list = await query.OrderBy(p => p.DayNha).ThenBy(p => p.TenPhong).ToListAsync();
            
            int currentTiet = GetCurrentTiet();
            var today = DateTime.Now.Date;

            foreach (var phong in list)
            {
                // Only override status if it's not permanently out of order
                if (phong.TrangThai != "Ngừng sử dụng" && phong.TrangThai != "Đang bảo trì")
                {
                    var currentSchedule = phong.LichSuDungPhongs.FirstOrDefault(l => 
                        l.NgaySuDung.Date == today && 
                        l.TrangThai == "Đã duyệt" && 
                        currentTiet >= l.TietBatDau && currentTiet <= l.TietKetThuc);
                        
                    if (currentSchedule != null)
                    {
                        phong.TrangThai = $"Đang sử dụng (Tiết {currentSchedule.TietBatDau}-{currentSchedule.TietKetThuc})";
                    }
                    else
                    {
                        phong.TrangThai = "Trống";
                    }
                }
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                if (trangThai == "Đang sử dụng")
                {
                    list = list.Where(p => p.TrangThai.StartsWith("Đang sử dụng")).ToList();
                }
                else
                {
                    list = list.Where(p => p.TrangThai == trangThai).ToList();
                }
            }

            return View(list);
        }

        private int GetCurrentTiet()
        {
            var hour = DateTime.Now.Hour;
            var minute = DateTime.Now.Minute;
            var time = hour + minute / 60.0;
            
            if (time >= 7.0 && time < 8.0) return 1;
            if (time >= 8.0 && time < 9.0) return 2;
            if (time >= 9.0 && time < 10.0) return 3;
            if (time >= 10.0 && time < 11.0) return 4;
            if (time >= 11.0 && time < 12.0) return 5;
            
            if (time >= 13.0 && time < 14.0) return 6;
            if (time >= 14.0 && time < 15.0) return 7;
            if (time >= 15.0 && time < 16.0) return 8;
            if (time >= 16.0 && time < 17.0) return 9;
            if (time >= 17.0 && time < 18.0) return 10;
            
            if (time >= 18.0 && time < 19.0) return 11;
            if (time >= 19.0 && time < 20.0) return 12;
            
            return 0; // Not in any class period
        }

        // GET: Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PhongHoc phong)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.PHONGHOC.AnyAsync(p => p.TenPhong == phong.TenPhong);
                if (exists)
                {
                    TempData["Error"] = "Tên phòng đã tồn tại.";
                    return View(phong);
                }

                phong.TrangThai = string.IsNullOrEmpty(phong.TrangThai) ? "Trống" : phong.TrangThai;
                _context.PHONGHOC.Add(phong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm phòng học thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(phong);
        }

        // GET: Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var phong = await _context.PHONGHOC.FindAsync(id);
            if (phong == null) return NotFound();
            return View(phong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, PhongHoc phong)
        {
            if (id != phong.MaPhong) return BadRequest();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.PHONGHOC.Update(phong);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật phòng học thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    TempData["Error"] = "Cập nhật thất bại.";
                }
            }
            return View(phong);
        }

        // GET: Details
        public async Task<IActionResult> Details(int id)
        {
            var phong = await _context.PHONGHOC.Include(p => p.ThietBis).FirstOrDefaultAsync(p => p.MaPhong == id);
            if (phong == null) return NotFound();
            return View(phong);
        }

        // POST: NgungSuDung
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> NgungSuDung(int id)
        {
            var phong = await _context.PHONGHOC.FindAsync(id);
            if (phong == null) return NotFound();
            phong.TrangThai = "Ngừng sử dụng";
            _context.PHONGHOC.Update(phong);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật trạng thái phòng thành 'Ngừng sử dụng'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
