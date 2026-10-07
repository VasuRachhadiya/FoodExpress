using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodExpress.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        [NotMapped]
        public decimal SubTotal => Items.Sum(i => i.Quantity * i.Price);

        [NotMapped]
        public int TotalItemCount => Items.Sum(i => i.Quantity);
    }

    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }

        [Required]
        public int FoodItemId { get; set; }

        [ForeignKey("FoodItemId")]
        public virtual FoodItem? FoodItem { get; set; }

        [Range(1, 99)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        public decimal Total => Quantity * Price;
    }
}
