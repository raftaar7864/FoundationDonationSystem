using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.ViewModels
{
    public class FoundationSettingViewModel
    {
        [Display(Name = "Foundation Name")]
        [StringLength(200)]
        public string FoundationName { get; set; } = "Foundation";
        [Display(Name = "Tagline")]
        [StringLength(300)]
        public string? Tagline { get; set; }
        [Display(Name = "Address")]
        [StringLength(1000)]
        public string? Address { get; set; }
        [Display(Name = "Phone")]
        [StringLength(30)]
        public string? Phone { get; set; }
        [Display(Name = "Email")]
        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }
        [Display(Name = "Website")]
        [StringLength(300)]
        public string? Website { get; set; }
        [Display(Name = "Registration Number")]
        [StringLength(150)]
        public string? RegistrationNumber { get; set; }
        [Display(Name = "PAN")]
        [StringLength(30)]
        public string? PAN { get; set; }
        [Display(Name = "Logo Path")]
        [StringLength(500)]
        public string? LogoPath { get; set; }
        [Display(Name = "Receipt Footer")]
        [StringLength(1000)]
        public string? ReceiptFooter { get; set; }
        [Display(Name = "Thank You Message")]
        [StringLength(1000)]
        public string? ThankYouMessage { get; set; }
    }
}