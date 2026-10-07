using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodExpress.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        [Display(Name = "Item Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be greater than zero.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [StringLength(255)]
        public string? Image { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual FoodCategory? Category { get; set; }

        [Required]
        [Display(Name = "Restaurant")]
        public int RestaurantId { get; set; }

        [ForeignKey("RestaurantId")]
        public virtual Restaurant? Restaurant { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        [NotMapped]
        public double AverageRating => Reviews.Any(r => r.IsApproved) ? Math.Round(Reviews.Where(r => r.IsApproved).Average(r => r.Rating), 1) : 0;
    }
}
