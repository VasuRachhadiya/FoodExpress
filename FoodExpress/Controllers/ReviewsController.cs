using FoodExpress.Data;
using FoodExpress.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(FoodExpressDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int restaurantId, int? foodItemId, int rating, string comment, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Please provide a rating between 1 and 5 stars.";
                return RedirectBack(returnUrl, restaurantId);
            }

            if (string.IsNullOrWhiteSpace(comment) || comment.Trim().Length < 5)
            {
                TempData["ErrorMessage"] = "Review comment must be at least 5 characters long.";
                return RedirectBack(returnUrl, restaurantId);
            }

            // Check if user already reviewed this restaurant in the last 24 hours to prevent spam
            var recentReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.RestaurantId == restaurantId && r.CreatedDate > DateTime.UtcNow.AddDays(-1));

            if (recentReview != null)
            {
                TempData["ErrorMessage"] = "You have already reviewed this restaurant recently. Thank you!";
                return RedirectBack(returnUrl, restaurantId);
            }

            var review = new Review
            {
                UserId = user.Id,
                RestaurantId = restaurantId,
                FoodItemId = foodItemId,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedDate = DateTime.UtcNow,
                IsApproved = true // Auto-approved or can be moderated by Admin
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you! Your review has been submitted.";
            return RedirectBack(returnUrl, restaurantId);
        }

        private IActionResult RedirectBack(string? returnUrl, int restaurantId)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Details", "Restaurants", new { id = restaurantId });
        }
    }
}
