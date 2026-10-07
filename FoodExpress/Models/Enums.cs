using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Pending")]
        Pending,

        [Display(Name = "Confirmed")]
        Confirmed,

        [Display(Name = "Preparing")]
        Preparing,

        [Display(Name = "Out For Delivery")]
        OutForDelivery,

        [Display(Name = "Delivered")]
        Delivered,

        [Display(Name = "Cancelled")]
        Cancelled
    }

    public enum PaymentStatus
    {
        [Display(Name = "Pending")]
        Pending,

        [Display(Name = "Paid")]
        Paid,

        [Display(Name = "Failed")]
        Failed,

        [Display(Name = "Refunded")]
        Refunded
    }

    public enum PaymentMethod
    {
        [Display(Name = "Cash on Delivery")]
        CashOnDelivery = 0,

        [Display(Name = "Online Payment")]
        OnlinePayment = 1,

        [Display(Name = "Online Payment (Simulated)")]
        OnlineSimulated = 1
    }

    public enum DiscountType
    {
        [Display(Name = "Percentage")]
        Percentage,

        [Display(Name = "Fixed Amount")]
        FixedAmount
    }
}
