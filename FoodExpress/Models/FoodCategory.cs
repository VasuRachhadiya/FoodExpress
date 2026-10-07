using System.ComponentModel.DataAnnotations;

namespace FoodExpress.Models
{
    public class FoodCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? Image { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
    }
}
