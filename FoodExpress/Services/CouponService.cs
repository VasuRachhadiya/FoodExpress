using FoodExpress.Data;
using FoodExpress.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Services
{
    public interface ICouponService
    {
        Task<(bool IsValid, decimal DiscountAmount, string Message)> ValidateCouponAsync(string code, decimal subTotal);
    }

    public class CouponService : ICouponService
    {
        private readonly FoodExpressDbContext _context;

        public CouponService(FoodExpressDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, decimal DiscountAmount, string Message)> ValidateCouponAsync(string code, decimal subTotal)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return (false, 0, "Please enter a valid coupon code.");
            }

            var cleanCode = code.Trim().ToUpperInvariant();
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == cleanCode);

            if (coupon == null)
            {
                return (false, 0, "Invalid coupon code.");
            }

            if (!coupon.IsActive)
            {
                return (false, 0, "This coupon is currently inactive.");
            }

            var today = DateTime.UtcNow.Date;
            if (today < coupon.StartDate.Date || today > coupon.EndDate.Date)
            {
                return (false, 0, $"This coupon is expired or not yet active (valid from {coupon.StartDate:MMM dd} to {coupon.EndDate:MMM dd}).");
            }

            if (subTotal < coupon.MinimumOrderAmount)
            {
                return (false, 0, $"Minimum order amount for this coupon is ${coupon.MinimumOrderAmount:F2}.");
            }

            decimal discount = 0;
            if (coupon.DiscountType == DiscountType.Percentage)
            {
                discount = Math.Round((subTotal * coupon.DiscountValue) / 100m, 2);
            }
            else
            {
                discount = Math.Min(coupon.DiscountValue, subTotal);
            }

            return (true, discount, $"Coupon applied successfully! Saved ${discount:F2}.");
        }
    }
}
