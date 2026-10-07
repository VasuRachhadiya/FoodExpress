using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodExpress.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Coupon Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Discount Type")]
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000.00, ErrorMessage = "Discount value must be greater than zero.")]
        [Display(Name = "Discount Value")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000.00)]
        [Display(Name = "Minimum Order Amount")]
        public decimal MinimumOrderAmount { get; set; } = 0;

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddMonths(1);

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        public bool IsCurrentlyValid => IsActive && DateTime.UtcNow.Date >= StartDate.Date && DateTime.UtcNow.Date <= EndDate.Date;
    }
}
