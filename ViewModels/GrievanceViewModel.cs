using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.ViewModels
{
    public class GrievanceViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        [Required, StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;
        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }
        [Required]
        public string Category { get; set; } = string.Empty;
        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        [Required, StringLength(5000)]
        public string Description { get; set; } = string.Empty;
        [StringLength(100)]
        [Display(Name = "Reference / Request Number")]
        public string? ReferenceNumber { get; set; }
    }
}
