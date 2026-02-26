using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hoc_Lieu_Va_Review_Demooo.Models;
using Microsoft.AspNetCore.Authorization;

namespace Hoc_Lieu_Va_Review_Demooo.Controllers
{
    // Yêu cầu đăng nhập. (Sau này phân quyền ta sẽ thêm [Authorize(Roles = "GiangVien,Admin")])
    [Authorize]
    public class KiemDuyetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KiemDuyetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị Dashboard Kiểm duyệt
        public async Task<IActionResult> Index()
        {
            // Chỉ lấy những tài liệu có trạng thái "ChoDuyet"
            var taiLieuChoDuyet = await _context.TaiLieus
                .Include(t => t.HocPhan)
                .Include(t => t.NguoiDung)
                .Where(t => t.TrangThaiDuyet == "ChoDuyet")
                .OrderBy(t => t.NgayUpload) // Cũ nhất xếp trên để duyệt trước
                .ToListAsync();

            // Truyền dữ liệu sang View thông qua ViewBag (để sau này dễ thêm Review, Báo cáo vào chung 1 trang)
            ViewBag.TaiLieuChoDuyet = taiLieuChoDuyet;

            return View();
        }

        // 2. [POST] Hành động DUYỆT tài liệu
        [HttpPost]
        public async Task<IActionResult> DuyetTaiLieu(int id)
        {
            var taiLieu = await _context.TaiLieus.FindAsync(id);
            if (taiLieu != null)
            {
                taiLieu.TrangThaiDuyet = "HopLe"; // Đổi trạng thái thành Hợp lệ
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index)); // Quay lại trang Dashboard
        }

        // 3. [POST] Hành động TỪ CHỐI tài liệu
        [HttpPost]
        public async Task<IActionResult> TuChoiTaiLieu(int id)
        {
            var taiLieu = await _context.TaiLieus.FindAsync(id);
            if (taiLieu != null)
            {
                taiLieu.TrangThaiDuyet = "TuChoi"; // Đổi trạng thái thành Từ chối
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}