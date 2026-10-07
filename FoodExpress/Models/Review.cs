using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodExpress.Models
{
    public class Review
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

        public int? FoodItemId { get; set; }

        [ForeignKey("FoodItemId")]
        public virtual FoodItem? FoodItem { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; } = 5;

        [Required]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Comment must be between 5 and 1000 characters.")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; } = true;
    }
}
