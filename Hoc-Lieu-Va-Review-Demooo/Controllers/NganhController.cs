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

        // 1. Hiển thị danh sách Ngành
        public async Task<IActionResult> Index()
        {
            // Dùng Include để lấy luôn thông tin của bảng Khoa (tránh bị rỗng tên khoa khi hiển thị)
            var danhSachNganh = await _context.Nganhs.Include(n => n.Khoa).ToListAsync();
            return View(danhSachNganh);
        }

        // 2. [GET] Hiển thị form Thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            // Tạo một danh sách thả xuống (SelectList) chứa các Khoa để chọn
            // "KhoaID" là giá trị ẩn (value), "TenKhoa" là chữ hiển thị ra ngoài (text)
            ViewData["KhoaID"] = new SelectList(_context.Khoas, "KhoaID", "TenKhoa");
            return View();
        }

        // 3. [POST] Lưu Ngành mới vào Database
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
    }
}