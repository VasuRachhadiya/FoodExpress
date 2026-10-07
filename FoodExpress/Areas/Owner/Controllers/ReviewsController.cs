using FoodExpress.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "RestaurantOwner")]
    public class ReviewsController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly UserManager<FoodExpress.Models.ApplicationUser> _userManager;

        public ReviewsController(FoodExpressDbContext context, UserManager<FoodExpress.Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Owner/Reviews
        public async Task<IActionResult> Index(int? restaurantId)
        {
            var userId = _userManager.GetUserId(User);
            var ownedRestaurants = await _context.Restaurants
                .Where(r => r.OwnerId == userId)
                .ToListAsync();

            var ownedIds = ownedRestaurants.Select(r => r.Id).ToList();

            var query = _context.Reviews
                .Where(r => ownedIds.Contains(r.RestaurantId))
                .Include(r => r.Restaurant)
                .Include(r => r.User)
                .Include(r => r.FoodItem)
                .AsQueryable();

            if (restaurantId.HasValue && restaurantId.Value > 0)
            {
                query = query.Where(r => r.RestaurantId == restaurantId.Value);
            }

            ViewBag.Restaurants = ownedRestaurants;
            ViewBag.SelectedRestaurantId = restaurantId;

            return View(await query.OrderByDescending(r => r.CreatedDate).ToListAsync());
        }
    }
}
