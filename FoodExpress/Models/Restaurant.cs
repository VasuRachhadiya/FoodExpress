using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodExpress.Models
{
    public class Restaurant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        [Display(Name = "Restaurant Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile / Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [RegularExpression(@"^(\+91[\-\s]?)?[6-9]\d{4}[\-\s]?\d{5}$", ErrorMessage = "Please enter a valid 10-digit Indian mobile number (e.g. +91 98765 43210 or 9876543210).")]
        [StringLength(25)]
        [Display(Name = "Mobile / Phone")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Image { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Opening Time")]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; } = new TimeSpan(9, 0, 0);

        [Display(Name = "Closing Time")]
        [DataType(DataType.Time)]
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(22, 0, 0);

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser? Owner { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        [NotMapped]
        public double AverageRating => Reviews.Any(r => r.IsApproved) ? Math.Round(Reviews.Where(r => r.IsApproved).Average(r => r.Rating), 1) : 0;

        [NotMapped]
        public double Rating
        {
            get => AverageRating;
            set { }
        }

        [NotMapped]
        public int ApprovedReviewCount => Reviews.Count(r => r.IsApproved);

        [NotMapped]
        public string FormattedPhone => FormatIndianPhoneNumber(Phone);

        public static string FormatIndianPhoneNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (digits.Length == 12 && digits.StartsWith("91"))
            {
                return $"+91 {digits.Substring(2, 5)} {digits.Substring(7, 5)}";
            }
            if (digits.Length == 11 && digits.StartsWith("0"))
            {
                return $"+91 {digits.Substring(1, 5)} {digits.Substring(6, 5)}";
            }
            if (digits.Length >= 10)
            {
                var last10 = digits.Substring(digits.Length - 10);
                return $"+91 {last10.Substring(0, 5)} {last10.Substring(5, 5)}";
            }
            return input.Trim();
        }
    }
}
