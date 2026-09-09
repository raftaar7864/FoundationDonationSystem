using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.ViewModels
{
    public class SupportRequestViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        [Required, StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;
        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }
        [StringLength(300)]
        public string? Address { get; set; }
        [Required]
        [Display(Name = "Support Category")]
        public string SupportCategory { get; set; } = string.Empty;
        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        [Required, StringLength(4000)]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Preferred Contact")]
        public string PreferredContactMethod { get; set; } = "Phone";
    }
}
