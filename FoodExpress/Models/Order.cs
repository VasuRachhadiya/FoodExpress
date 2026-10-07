using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodExpress.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        public int RestaurantId { get; set; }

        [ForeignKey("RestaurantId")]
        public virtual Restaurant? Restaurant { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal DiscountAmount { get; set; } = 0;

        [StringLength(50)]
        public string? CouponCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal DeliveryFee { get; set; } = 49.00m;

        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(25)]
        [Display(Name = "Contact Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [StringLength(500)]
        public string? Notes { get; set; }

        [NotMapped]
        public string? OrderNotes
        {
            get => Notes;
            set => Notes = value;
        }

        // Navigation
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [Required]
        public int FoodItemId { get; set; }

        [ForeignKey("FoodItemId")]
        public virtual FoodItem? FoodItem { get; set; }

        [Range(1, 99)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        public decimal UnitPrice
        {
            get => Price;
            set => Price = value;
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }
    }
}
