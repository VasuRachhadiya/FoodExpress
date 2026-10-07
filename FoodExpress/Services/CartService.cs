using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Services
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(string userId);
        Task<CartViewModel> GetCartViewModelAsync(string userId, string? couponCode = null);
        Task<(bool Success, string Message, bool ConflictWithOtherRestaurant, string? ExistingRestaurantName)> AddItemAsync(
            string userId, int foodItemId, int quantity = 1, bool forceClear = false);
        Task<bool> UpdateQuantityAsync(string userId, int cartItemId, int quantity);
        Task<bool> RemoveItemAsync(string userId, int cartItemId);
        Task<bool> ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
    }

    public class CartService : ICartService
    {
        private readonly FoodExpressDbContext _context;
        private readonly ICouponService _couponService;

        public CartService(FoodExpressDbContext context, ICouponService couponService)
        {
            _context = context;
            _couponService = couponService;
        }

        public async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.FoodItem)
                        .ThenInclude(f => f!.Restaurant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartViewModel> GetCartViewModelAsync(string userId, string? couponCode = null)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var vm = new CartViewModel
            {
                Items = cart.Items.Select(i => new CartItemViewModel
                {
                    Id = i.Id,
                    FoodItemId = i.FoodItemId,
                    Name = i.FoodItem?.Name ?? "Item",
                    Image = i.FoodItem?.Image,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    RestaurantId = i.FoodItem?.RestaurantId ?? 0,
                    RestaurantName = i.FoodItem?.Restaurant?.Name ?? "Restaurant"
                }).ToList()
            };

            if (vm.Items.Any())
            {
                var firstItem = vm.Items.First();
                vm.RestaurantId = firstItem.RestaurantId;
                vm.RestaurantName = firstItem.RestaurantName;
            }

            // Coupon validation if provided
            if (!string.IsNullOrWhiteSpace(couponCode) && vm.SubTotal > 0)
            {
                var couponResult = await _couponService.ValidateCouponAsync(couponCode, vm.SubTotal);
                vm.CouponCode = couponCode.Trim().ToUpperInvariant();
                vm.IsCouponValid = couponResult.IsValid;
                vm.CouponMessage = couponResult.Message;
                if (couponResult.IsValid)
                {
                    vm.DiscountAmount = couponResult.DiscountAmount;
                }
            }

            return vm;
        }

        public async Task<(bool Success, string Message, bool ConflictWithOtherRestaurant, string? ExistingRestaurantName)> AddItemAsync(
            string userId, int foodItemId, int quantity = 1, bool forceClear = false)
        {
            var foodItem = await _context.FoodItems
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == foodItemId);

            if (foodItem == null)
            {
                return (false, "Food item not found.", false, null);
            }

            if (!foodItem.IsAvailable)
            {
                return (false, "This food item is currently out of stock / unavailable.", false, null);
            }

            if (foodItem.Restaurant == null || !foodItem.Restaurant.IsActive)
            {
                return (false, "This restaurant is currently closed or inactive.", false, null);
            }

            var cart = await GetOrCreateCartAsync(userId);

            // Check if cart contains items from a different restaurant
            var existingFirstItem = cart.Items.FirstOrDefault();
            if (existingFirstItem != null && existingFirstItem.FoodItem?.RestaurantId != foodItem.RestaurantId)
            {
                var existingRestaurant = existingFirstItem.FoodItem?.Restaurant?.Name ?? "another restaurant";

                if (!forceClear)
                {
                    // Business rule: one restaurant per order. Request user confirmation to replace
                    return (false, $"Your cart contains dishes from '{existingRestaurant}'. FoodExpress allows ordering from one restaurant at a time.", true, existingRestaurant);
                }

                // Force clear selected by user
                _context.CartItems.RemoveRange(cart.Items);
                cart.Items.Clear();
            }

            // Check if item already in cart
            var existingCartItem = cart.Items.FirstOrDefault(i => i.FoodItemId == foodItemId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                existingCartItem.Price = foodItem.Price; // keep price fresh
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    FoodItemId = foodItemId,
                    Quantity = quantity,
                    Price = foodItem.Price
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return (true, $"{foodItem.Name} added to cart!", false, null);
        }

        public async Task<bool> UpdateQuantityAsync(string userId, int cartItemId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item == null) return false;

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = Math.Min(99, quantity);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveItemAsync(string userId, int cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            if (cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _context.CartItems
                .Where(ci => ci.Cart!.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }
    }
}
