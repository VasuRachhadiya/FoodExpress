using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "RestaurantOwner")]
    public class OrdersController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(
            FoodExpressDbContext context,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _orderService = orderService;
            _userManager = userManager;
        }

        // GET: /Owner/Orders
        public async Task<IActionResult> Index(OrderStatus? status, int? restaurantId)
        {
            var userId = _userManager.GetUserId(User);
            var ownedRestaurants = await _context.Restaurants
                .Where(r => r.OwnerId == userId)
                .ToListAsync();

            var ownedIds = ownedRestaurants.Select(r => r.Id).ToList();

            var query = _context.Orders
                .Where(o => ownedIds.Contains(o.RestaurantId))
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.OrderStatus == status.Value);
            }

            if (restaurantId.HasValue && restaurantId.Value > 0)
            {
                query = query.Where(o => o.RestaurantId == restaurantId.Value);
            }

            ViewBag.Restaurants = ownedRestaurants;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedRestaurantId = restaurantId;

            return View(await query.OrderByDescending(o => o.OrderDate).ToListAsync());
        }

        // GET: /Owner/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefaultAsync(o => o.Id == id && o.Restaurant!.OwnerId == userId);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Owner/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus orderStatus)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == id && o.Restaurant!.OwnerId == userId);

            if (order == null) return NotFound();

            order.OrderStatus = orderStatus;
            if (orderStatus == OrderStatus.Delivered && order.PaymentStatus == PaymentStatus.Pending)
            {
                order.PaymentStatus = PaymentStatus.Paid;
            }
            else if (orderStatus == OrderStatus.Cancelled && order.PaymentStatus == PaymentStatus.Paid)
            {
                order.PaymentStatus = PaymentStatus.Refunded;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Order #{id} status updated to {orderStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
