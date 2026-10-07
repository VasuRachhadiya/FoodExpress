using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly IOrderService _orderService;

        public OrdersController(FoodExpressDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Index(OrderStatus? status, PaymentStatus? paymentStatus, DateTime? date)
        {
            var query = _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.OrderStatus == status.Value);
            }

            if (paymentStatus.HasValue)
            {
                query = query.Where(o => o.PaymentStatus == paymentStatus.Value);
            }

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(o => o.OrderDate.Date == targetDate);
            }

            ViewBag.SelectedStatus = status;
            ViewBag.SelectedPaymentStatus = paymentStatus;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

            return View(await query.OrderByDescending(o => o.OrderDate).ToListAsync());
        }

        // GET: /Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id, isStaff: true);
            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus orderStatus, PaymentStatus? paymentStatus)
        {
            var updated = await _orderService.UpdateOrderStatusAsync(id, orderStatus, paymentStatus);
            if (updated)
            {
                TempData["SuccessMessage"] = $"Order #{id} status successfully updated to {orderStatus}.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Order #{id} could not be updated.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
