using FoodExpress.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly FoodExpressDbContext _context;

        public ReviewsController(FoodExpressDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Reviews
        public async Task<IActionResult> Index(bool? approved)
        {
            var query = _context.Reviews
                .Include(r => r.Restaurant)
                .Include(r => r.User)
                .Include(r => r.FoodItem)
                .AsQueryable();

            if (approved.HasValue)
            {
                query = query.Where(r => r.IsApproved == approved.Value);
            }

            ViewBag.ApprovedFilter = approved;
            return View(await query.OrderByDescending(r => r.CreatedDate).ToListAsync());
        }

        // POST: /Admin/Reviews/ToggleApproval/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApproval(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.IsApproved = !review.IsApproved;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Review status changed to {(review.IsApproved ? "Approved" : "Pending/Rejected")}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Review has been removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
