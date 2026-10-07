using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Services
{
    public interface IOrderService
    {
        Task<(bool Success, string Message, int? OrderId)> CreateOrderAsync(string userId, CheckoutViewModel checkoutVm);
        Task<List<OrderListViewModel>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderByIdAsync(int orderId, string? userId = null, bool isStaff = false);
        Task<(bool Success, string Message)> CancelOrderAsync(int orderId, string userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, PaymentStatus? newPaymentStatus = null);
        Task<List<OrderListViewModel>> GetRestaurantOrdersAsync(int restaurantId);
        Task<List<OrderListViewModel>> GetOwnerOrdersAsync(string ownerId);
    }

    public class OrderService : IOrderService
    {
        private readonly FoodExpressDbContext _context;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public OrderService(FoodExpressDbContext context, ICartService cartService, ICouponService couponService)
        {
            _context = context;
            _cartService = cartService;
            _couponService = couponService;
        }

        public async Task<(bool Success, string Message, int? OrderId)> CreateOrderAsync(string userId, CheckoutViewModel checkoutVm)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.FoodItem)
                        .ThenInclude(f => f!.Restaurant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return (false, "Your shopping cart is empty.", null);
            }

            // Verify availability of each item and restaurant active status
            var firstItem = cart.Items.First();
            var restaurantId = firstItem.FoodItem?.RestaurantId ?? 0;
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null || !restaurant.IsActive)
            {
                return (false, "The restaurant for this order is currently closed or inactive.", null);
            }

            foreach (var item in cart.Items)
            {
                if (item.FoodItem == null || !item.FoodItem.IsAvailable)
                {
                    return (false, $"Item '{item.FoodItem?.Name ?? "Selected item"}' is no longer available.", null);
                }
            }

            var subTotal = cart.Items.Sum(i => i.Quantity * i.Price);
            decimal discountAmount = 0;
            string? validCouponCode = null;

            if (!string.IsNullOrWhiteSpace(checkoutVm.CouponCode))
            {
                var couponCheck = await _couponService.ValidateCouponAsync(checkoutVm.CouponCode, subTotal);
                if (couponCheck.IsValid)
                {
                    discountAmount = couponCheck.DiscountAmount;
                    validCouponCode = checkoutVm.CouponCode.Trim().ToUpperInvariant();
                }
            }

            decimal deliveryFee = 49.00m;
            decimal totalAmount = Math.Max(0, subTotal - discountAmount + deliveryFee);

            var order = new Order
            {
                UserId = userId,
                RestaurantId = restaurantId,
                OrderDate = DateTime.UtcNow,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                CouponCode = validCouponCode,
                DeliveryFee = deliveryFee,
                TotalAmount = totalAmount,
                DeliveryAddress = checkoutVm.DeliveryAddress.Trim(),
                PhoneNumber = checkoutVm.PhoneNumber.Trim(),
                PaymentMethod = checkoutVm.PaymentMethod,
                PaymentStatus = checkoutVm.PaymentMethod == PaymentMethod.OnlinePayment ? PaymentStatus.Paid : PaymentStatus.Pending,
                OrderStatus = OrderStatus.Pending,
                Notes = checkoutVm.Notes?.Trim()
            };

            foreach (var item in cart.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    FoodItemId = item.FoodItemId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    SubTotal = item.Quantity * item.Price
                });
            }

            _context.Orders.Add(order);

            // Clear Cart
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();
            return (true, "Order placed successfully!", order.Id);
        }

        public async Task<List<OrderListViewModel>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    RestaurantId = o.RestaurantId,
                    RestaurantName = o.Restaurant != null ? o.Restaurant.Name : "Restaurant",
                    RestaurantImage = o.Restaurant != null ? o.Restaurant.Image : null,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod,
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    CustomerName = o.User != null ? o.User.FullName : "Customer"
                }).ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId, string? userId = null, bool isStaff = false)
        {
            var query = _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                .AsQueryable();

            if (!isStaff && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(o => o.UserId == userId);
            }

            return await query.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, string userId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return (false, "Order not found or access denied.");
            }

            if (order.OrderStatus != OrderStatus.Pending)
            {
                return (false, "Only orders with 'Pending' status can be cancelled.");
            }

            order.OrderStatus = OrderStatus.Cancelled;
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                order.PaymentStatus = PaymentStatus.Refunded;
            }

            await _context.SaveChangesAsync();
            return (true, "Order has been cancelled successfully.");
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, PaymentStatus? newPaymentStatus = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.OrderStatus = newStatus;
            if (newPaymentStatus.HasValue)
            {
                order.PaymentStatus = newPaymentStatus.Value;
            }
            else if (newStatus == OrderStatus.Delivered && order.PaymentStatus == PaymentStatus.Pending)
            {
                // If delivered on Cash on Delivery, mark as paid
                order.PaymentStatus = PaymentStatus.Paid;
            }
            else if (newStatus == OrderStatus.Cancelled && order.PaymentStatus == PaymentStatus.Paid)
            {
                order.PaymentStatus = PaymentStatus.Refunded;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<OrderListViewModel>> GetRestaurantOrdersAsync(int restaurantId)
        {
            return await _context.Orders
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    RestaurantId = o.RestaurantId,
                    RestaurantName = o.Restaurant != null ? o.Restaurant.Name : "Restaurant",
                    RestaurantImage = o.Restaurant != null ? o.Restaurant.Image : null,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod,
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    CustomerName = o.User != null ? o.User.FullName : "Customer"
                }).ToListAsync();
        }

        public async Task<List<OrderListViewModel>> GetOwnerOrdersAsync(string ownerId)
        {
            return await _context.Orders
                .Where(o => o.Restaurant!.OwnerId == ownerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    RestaurantId = o.RestaurantId,
                    RestaurantName = o.Restaurant != null ? o.Restaurant.Name : "Restaurant",
                    RestaurantImage = o.Restaurant != null ? o.Restaurant.Image : null,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod,
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    CustomerName = o.User != null ? o.User.FullName : "Customer"
                }).ToListAsync();
        }
    }
}
