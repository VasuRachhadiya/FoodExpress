using FoodExpress.Models;
using FoodExpress.Services;
using FoodExpress.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var orders = await _orderService.GetUserOrdersAsync(user.Id);
            return View(orders);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _orderService.GetOrderByIdAsync(id, user.Id, isStaff: false);
            if (order == null)
            {
                return NotFound();
            }

            var vm = new OrderDetailViewModel { Order = order };
            return View(vm);
        }

        // GET: /Orders/Track/5
        public async Task<IActionResult> Track(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _orderService.GetOrderByIdAsync(id, user.Id, isStaff: false);
            if (order == null)
            {
                return NotFound();
            }

            var vm = new OrderDetailViewModel { Order = order };
            return View(vm);
        }

        // POST: /Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _orderService.CancelOrderAsync(id, user.Id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
