using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review_Demooo.Models;
using Microsoft.AspNetCore.Authorization;

namespace Hoc_Lieu_Va_Review_Demooo.Controllers
{
    [Authorize] // Phải đăng nhập
    public class NganhController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NganhController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách Ngành
        public async Task<IActionResult> Index()
        {
            // Dùng Include để lấy luôn thông tin của bảng Khoa (tránh bị rỗng tên khoa khi hiển thị)
            var danhSachNganh = await _context.Nganhs.Include(n => n.Khoa).ToListAsync();
            return View(danhSachNganh);
        }

        // [GET] Hiển thị form Thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            // Tạo một danh sách thả xuống (SelectList) chứa các Khoa để chọn
            // "KhoaID" là giá trị ẩn (value), "TenKhoa" là chữ hiển thị ra ngoài (text)
            ViewData["KhoaID"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa");
            return View();
        }

        // [POST] Lưu Ngành mới vào Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenNganh,MoTa,KhoaID")] Nganh nganh)
        {
            // Yêu cầu hệ thống không bắt lỗi thiếu dữ liệu của object Khoa
            ModelState.Remove("Khoa");

            if (ModelState.IsValid)
            {
                _context.Add(nganh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Nếu lỗi, phải nạp lại danh sách Khoa cho dropdown kẻo bị lỗi giao diện
            ViewData["KhoaID"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa", nganh.KhoaID);
            return View(nganh);
        }

        // CHỨC NĂNG SỬA (EDIT) 
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nganh = await _context.Nganhs.FindAsync(id);
            if (nganh == null) return NotFound();

            // Truyền lại danh sách Khoa cho Dropdown, nhớ chọn sẵn Khoa hiện tại của Ngành này
            ViewData["KhoaID"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa", nganh.KhoaID);
            return View(nganh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NganhID,TenNganh,MoTa,KhoaID")] Nganh nganh)
        {
            if (id != nganh.NganhID) return NotFound();

            // Vẫn phải có dòng này để bỏ qua lỗi validate đối tượng Khoa nhé
            ModelState.Remove("Khoa");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nganh);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NganhExists(nganh.NganhID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["KhoaID"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa", nganh.KhoaID);
            return View(nganh);
        }

        private bool NganhExists(int id)
        {
            return _context.Nganhs.Any(e => e.NganhID == id);
        }

        // CHỨC NĂNG XÓA (DELETE)
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Dùng Include để lấy tên Khoa hiển thị ra màn hình xác nhận xóa cho đẹp
            var nganh = await _context.Nganhs
                .Include(n => n.Khoa)
                .FirstOrDefaultAsync(m => m.NganhID == id);

            if (nganh == null) return NotFound();

            return View(nganh);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nganh = await _context.Nganhs.FindAsync(id);
            if (nganh != null)
            {
                _context.Nganhs.Remove(nganh);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}