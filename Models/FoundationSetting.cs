using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.Models
{
    public class FoundationSetting
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "Foundation Name")]
        public string FoundationName { get; set; } = string.Empty;
        [StringLength(300)]
        [Display(Name = "Tagline")]
        public string? Tagline { get; set; }
        [StringLength(1000)]
        [Display(Name = "Address")]
        public string? Address { get; set; }
        [StringLength(30)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Email")]
        public string? Email { get; set; }
        [StringLength(300)]
        [Display(Name = "Website")]
        public string? Website { get; set; }
        [StringLength(150)]
        [Display(Name = "Registration Number")]
        public string? RegistrationNumber { get; set; }
        [StringLength(30)]
        [Display(Name = "PAN")]
        public string? PAN { get; set; }
        [StringLength(500)]
        [Display(Name = "Logo Path")]
        public string? LogoPath { get; set; }
        [StringLength(1000)]
        [Display(Name = "Receipt Footer")]
        public string? ReceiptFooter { get; set; }
        [StringLength(1000)]
        [Display(Name = "Thank You Message")]
        public string? ThankYouMessage { get; set; }
        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}