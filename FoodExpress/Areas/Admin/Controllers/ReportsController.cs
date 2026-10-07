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
    public class ReportsController : Controller
    {
        private readonly FoodExpressDbContext _context;

        public ReportsController(FoodExpressDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Reports
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                .Include(o => o.Restaurant)
                .ToListAsync();

            var totalOrdersCount = orders.Count;
            var completedOrders = orders.Where(o => o.OrderStatus == OrderStatus.Delivered).ToList();
            var cancelledOrders = orders.Where(o => o.OrderStatus == OrderStatus.Cancelled).ToList();
            var totalRevenue = orders.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.TotalAmount);

            var avgOrderValue = totalOrdersCount > 0 ? totalRevenue / totalOrdersCount : 0;

            // Top food items
            var topFoodItems = await _context.OrderItems
                .Include(oi => oi.FoodItem)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o!.Restaurant)
                .GroupBy(oi => new { oi.FoodItemId, FoodName = oi.FoodItem!.Name, RestaurantName = oi.FoodItem.Restaurant!.Name })
                .Select(g => new TopFoodReportItem
                {
                    FoodName = g.Key.FoodName,
                    RestaurantName = g.Key.RestaurantName,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.SubTotal)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToListAsync();

            // Restaurant sales
            var restaurantSales = orders
                .GroupBy(o => o.Restaurant?.Name ?? "Unknown")
                .Select(g => new RestaurantSalesReportItem
                {
                    RestaurantName = g.Key,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            // Daily sales for past 7 days
            var past7Days = Enumerable.Range(0, 7)
                .Select(offset => DateTime.UtcNow.Date.AddDays(-offset))
                .OrderBy(d => d)
                .ToList();

            var dailySales = past7Days.Select(day =>
            {
                var dayOrders = orders.Where(o => o.OrderDate.Date == day).ToList();
                return new DailySalesReportItem
                {
                    Date = day.ToString("MMM dd"),
                    OrdersCount = dayOrders.Count,
                    Revenue = dayOrders.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.TotalAmount)
                };
            }).ToList();

            var reportVm = new ReportViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrdersCount = totalOrdersCount,
                CompletedOrdersCount = completedOrders.Count,
                CancelledOrdersCount = cancelledOrders.Count,
                AverageOrderValue = avgOrderValue,
                TopFoodItems = topFoodItems,
                RestaurantSales = restaurantSales,
                DailySales = dailySales
            };

            return View(reportVm);
        }
    }
}
