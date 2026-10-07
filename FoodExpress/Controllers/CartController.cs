using FoodExpress.Models;
using FoodExpress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        // GET: /Cart
        public async Task<IActionResult> Index(string? couponCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cartVm = await _cartService.GetCartViewModelAsync(user.Id, couponCode);
            return View(cartVm);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int foodItemId, int quantity = 1, bool forceClear = false, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _cartService.AddItemAsync(user.Id, foodItemId, quantity, forceClear);

            if (result.ConflictWithOtherRestaurant)
            {
                TempData["CartConflict"] = result.Message;
                TempData["ConflictFoodItemId"] = foodItemId;
                TempData["ConflictQuantity"] = quantity;
                return RedirectToAction(nameof(Index));
            }

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _cartService.UpdateQuantityAsync(user.Id, cartItemId, quantity);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _cartService.RemoveItemAsync(user.Id, cartItemId);
            TempData["SuccessMessage"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _cartService.ClearCartAsync(user.Id);
            TempData["SuccessMessage"] = "Your cart has been cleared.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/GetCount (API endpoint for updating navbar badge live)
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0 });

            var count = await _cartService.GetCartItemCountAsync(user.Id);
            return Json(new { count });
        }
    }
}
