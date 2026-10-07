using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "RestaurantOwner")]
    public class DashboardController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(FoodExpressDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var restaurants = await _context.Restaurants
                .Where(r => r.OwnerId == userId)
                .Include(r => r.FoodItems)
                .Include(r => r.Orders)
                .ToListAsync();

            var restaurantIds = restaurants.Select(r => r.Id).ToList();

            var orders = await _context.Orders
                .Where(o => restaurantIds.Contains(o.RestaurantId))
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var foodItemsCount = await _context.FoodItems
                .CountAsync(f => restaurantIds.Contains(f.RestaurantId));

            var totalRevenue = orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid)
                .Sum(o => o.TotalAmount);

            var model = new OwnerDashboardViewModel
            {
                TotalRestaurants = restaurants.Count,
                TotalFoodItems = foodItemsCount,
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.OrderStatus == OrderStatus.Pending),
                PreparingOrders = orders.Count(o => o.OrderStatus == OrderStatus.Preparing || o.OrderStatus == OrderStatus.Confirmed),
                CompletedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered),
                TotalRevenue = totalRevenue,
                Restaurants = restaurants,
                RecentOrders = orders.Take(5).ToList()
            };

            return View(model);
        }
    }
}
