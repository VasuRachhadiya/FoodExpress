using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly FoodExpressDbContext _context;

        public DashboardController(FoodExpressDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalRestaurants = await _context.Restaurants.CountAsync();
            var totalFoodItems = await _context.FoodItems.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();

            var pendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Pending);
            var completedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Delivered);
            var cancelledOrders = await _context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Cancelled);

            var totalRevenue = await _context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount);

            var recentOrders = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(7)
                .ToListAsync();

            var topSellingFoods = await _context.FoodItems
                .Include(f => f.Restaurant)
                .Include(f => f.OrderItems)
                .OrderByDescending(f => f.OrderItems.Sum(oi => oi.Quantity))
                .Take(5)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalRestaurants = totalRestaurants,
                TotalFoodItems = totalFoodItems,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders,
                TopSellingFoods = topSellingFoods
            };

            return View(model);
        }
    }
}
