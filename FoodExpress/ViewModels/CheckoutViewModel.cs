using System.ComponentModel.DataAnnotations;
using FoodExpress.Models;

namespace FoodExpress.ViewModels
{
    public class CheckoutViewModel
    {
        public int? SelectedAddressId { get; set; }

        [Required(ErrorMessage = "Delivery address is required.")]
        [StringLength(250)]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact phone is required.")]
        [Phone]
        [StringLength(25)]
        [Display(Name = "Contact Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Delivery Notes (e.g. Ring doorbell, leave at gate)")]
        public string? Notes { get; set; }

        public string? OrderNotes { get => Notes; set => Notes = value; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        public string? CouponCode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DeliveryFee { get; set; } = 49.00m;
        public decimal TotalAmount { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;

        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public List<Address> SavedAddresses { get; set; } = new List<Address>();
    }
}
