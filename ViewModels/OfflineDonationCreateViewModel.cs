using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.ViewModels
{
    public class OfflineDonationCreateViewModel
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string DonorName { get; set; } = string.Empty;
        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;
        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }
        [StringLength(500)]
        public string? Address { get; set; }
        [Required]
        [Range(
            typeof(decimal),
            "1",
            "999999999",
            ErrorMessage = "Donation amount must be greater than 0.")]
        [Display(Name = "Donation Amount")]
        public decimal Amount { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Collection Date")]
        public DateTime CollectionDate { get; set; } = DateTime.Today;
        [Required]
        [Display(Name = "Offline Donation Voucher")]
        public IFormFile? OfflineVoucher { get; set; }
        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }
}