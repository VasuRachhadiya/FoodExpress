using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.Services;
using FoodExpress.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            FoodExpressDbContext context,
            ICartService cartService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        // GET: /Checkout
        public async Task<IActionResult> Index(string? couponCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cartVm = await _cartService.GetCartViewModelAsync(user.Id, couponCode);
            if (!cartVm.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Add food items before checking out.";
                return RedirectToAction("Index", "Cart");
            }

            var savedAddresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            var defaultAddress = savedAddresses.FirstOrDefault(a => a.IsDefault) ?? savedAddresses.FirstOrDefault();

            var checkoutVm = new CheckoutViewModel
            {
                Items = cartVm.Items,
                SubTotal = cartVm.SubTotal,
                DiscountAmount = cartVm.DiscountAmount,
                DeliveryFee = cartVm.DeliveryFee,
                TotalAmount = cartVm.TotalAmount,
                CouponCode = cartVm.CouponCode,
                RestaurantId = cartVm.RestaurantId ?? 0,
                RestaurantName = cartVm.RestaurantName ?? "Restaurant",
                SavedAddresses = savedAddresses,
                SelectedAddressId = defaultAddress?.Id,
                DeliveryAddress = defaultAddress != null ? $"{defaultAddress.AddressLine}, {defaultAddress.City}, {defaultAddress.State} {defaultAddress.PostalCode}" : (user.Address ?? ""),
                PhoneNumber = defaultAddress?.PhoneNumber ?? user.PhoneNumber ?? ""
            };

            return View(checkoutVm);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // If user selected an address from dropdown, populate from database
            if (model.SelectedAddressId.HasValue && model.SelectedAddressId.Value > 0)
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == model.SelectedAddressId.Value && a.UserId == user.Id);
                if (address != null)
                {
                    model.DeliveryAddress = $"{address.AddressLine}, {address.City}, {address.State} {address.PostalCode}";
                    model.PhoneNumber = address.PhoneNumber;
                }
            }

            if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
            {
                ModelState.AddModelError("DeliveryAddress", "Delivery address is required.");
            }

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Contact phone number is required.");
            }

            if (!ModelState.IsValid)
            {
                var cartVm = await _cartService.GetCartViewModelAsync(user.Id, model.CouponCode);
                model.Items = cartVm.Items;
                model.SubTotal = cartVm.SubTotal;
                model.DiscountAmount = cartVm.DiscountAmount;
                model.DeliveryFee = cartVm.DeliveryFee;
                model.TotalAmount = cartVm.TotalAmount;
                model.SavedAddresses = await _context.Addresses.Where(a => a.UserId == user.Id).ToListAsync();
                return View("Index", model);
            }

            var result = await _orderService.CreateOrderAsync(user.Id, model);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index", "Cart");
            }

            TempData["SuccessMessage"] = "Thank you! Your order has been placed successfully.";
            return RedirectToAction(nameof(Confirmation), new { id = result.OrderId });
        }

        // GET: /Checkout/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _orderService.GetOrderByIdAsync(id, user.Id, isStaff: false);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
