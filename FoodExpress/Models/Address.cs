using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodExpress.Models
{
    public class Address
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Address Title (e.g. Home, Work)")]
        public string Title { get; set; } = "Home";

        [Required]
        [StringLength(200)]
        [Display(Name = "Street Address")]
        public string AddressLine { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Postal / ZIP Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Contact Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Set as Default Address")]
        public bool IsDefault { get; set; } = false;
    }
}
