using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Controllers
{
    [Authorize]
    public class ThietBiController : Controller
    {
        private readonly QLPhongHocContext _context;
        public ThietBiController(QLPhongHocContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, int? maPhong, string tinhTrang)
        {
            var query = _context.THIETBI.Include(t => t.PhongHoc).AsQueryable();
            
            if (!string.IsNullOrEmpty(searchTerm)) 
                query = query.Where(t => t.TenThietBi.Contains(searchTerm));

            if (maPhong.HasValue) query = query.Where(t => t.MaPhong == maPhong.Value);
            if (!string.IsNullOrEmpty(tinhTrang)) query = query.Where(t => t.TinhTrang == tinhTrang);

            ViewBag.Phongs = new SelectList(await _context.PHONGHOC.ToListAsync(), "MaPhong", "TenPhong");
            return View(await query.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Phongs = new SelectList(_context.PHONGHOC, "MaPhong", "TenPhong");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ThietBi thietBi)
        {
            if (ModelState.IsValid)
            {
                _context.THIETBI.Add(thietBi);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm thiết bị thành công.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Phongs = new SelectList(_context.PHONGHOC, "MaPhong", "TenPhong", thietBi.MaPhong);
            return View(thietBi);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var tb = await _context.THIETBI.FindAsync(id);
            if (tb == null) return NotFound();
            ViewBag.Phongs = new SelectList(_context.PHONGHOC, "MaPhong", "TenPhong", tb.MaPhong);
            return View(tb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, ThietBi thietBi)
        {
            if (id != thietBi.MaThietBi) return BadRequest();
            if (ModelState.IsValid)
            {
                _context.THIETBI.Update(thietBi);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thiết bị thành công.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Phongs = new SelectList(_context.PHONGHOC, "MaPhong", "TenPhong", thietBi.MaPhong);
            return View(thietBi);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tb = await _context.THIETBI.Include(t => t.PhongHoc).FirstOrDefaultAsync(t => t.MaThietBi == id);
            if (tb == null) return NotFound();
            return View(tb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var tb = await _context.THIETBI.FindAsync(id);
            if (tb == null) return NotFound();

            var related = await _context.SUCO.AnyAsync(s => s.MaThietBi == id);
            if (!related)
            {
                _context.THIETBI.Remove(tb);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa thiết bị thành công.";
            }
            else
            {
                tb.TinhTrang = "Ngừng sử dụng";
                _context.THIETBI.Update(tb);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thiết bị đang liên quan sự cố, đã chuyển sang 'Ngừng sử dụng'.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
