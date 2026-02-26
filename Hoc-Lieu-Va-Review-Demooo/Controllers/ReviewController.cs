using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review_Demooo.Models;
using Microsoft.AspNetCore.Authorization;
// Bổ sung thư viện Services để gọi AI
using Hoc_Lieu_Va_Review_Demooo.Services;

namespace Hoc_Lieu_Va_Review_Demooo.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiService _geminiService; // 1. Khai báo trợ lý AI

        // 2. Tiêm AI vào Controller
        public ReviewController(ApplicationDbContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        // 1. Hiển thị danh sách các bài Review
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(r => r.HocPhan)
                .Include(r => r.NguoiDung)
                // 3. CHỈ LẤY CÁC BÀI REVIEW HỢP LỆ (Đã được AI hoặc Giảng viên duyệt)
                .Where(r => r.TrangThaiDuyet == "HopLe" || r.TrangThaiDuyet == "DaDuyet")
                .OrderByDescending(r => r.NgayDang)
                .ToListAsync();
            return View(reviews);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["HocPhanID"] = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HocPhanID,NoiDung,SoSao")] Review review)
        {
            ModelState.Remove("NguoiDung");
            ModelState.Remove("HocPhan");
            ModelState.Remove("TrangThaiDuyet");

            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim != null)
                {
                    review.NguoiDungID = int.Parse(userIdClaim.Value);
                }

                review.NgayDang = DateTime.Now;

                // 4. MỜI TRỢ LÝ AI VÀO DUYỆT BÀI REVIEW
                string ketQuaDuyet = await _geminiService.KiemDuyetVanBan(review.NoiDung);
                review.TrangThaiDuyet = ketQuaDuyet;

                _context.Add(review);
                await _context.SaveChangesAsync();

                // 5. Gửi thông báo bằng TempData để hiện popup xanh/đỏ bên ngoài giao diện
                if (ketQuaDuyet == "TuChoi")
                {
                    TempData["ThongBaoReview"] = "❌ Bài đánh giá chứa nội dung vi phạm và đã bị AI chặn!";
                }
                else if (ketQuaDuyet == "ChoDuyet")
                {
                    TempData["ThongBaoReview"] = "⏳ Bài đánh giá có từ ngữ lạ, đang chờ Giảng viên duyệt.";
                }
                else
                {
                    TempData["ThongBaoReview"] = "✅ Đăng bài đánh giá thành công!";
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["HocPhanID"] = new SelectList(_context.HocPhans, "HocPhanID", "TenHocPhan", review.HocPhanID);
            return View(review);
        }
    }
}