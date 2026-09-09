using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.ViewModels
{
    public class DonationSlipViewModel
    {
        public int Id { get; set; }
        public int DonationId { get; set; }
        [Display(Name = "Slip Number")]
        public string SlipNumber { get; set; } = string.Empty;
        [Display(Name = "Donation Number")]
        public string DonationNumber { get; set; } = string.Empty;
        [Display(Name = "Donor Name")]
        public string DonorName { get; set; } = string.Empty;
        [Display(Name = "Mobile")]
        public string Mobile { get; set; } = string.Empty;
        [Display(Name = "Email")]
        public string? Email { get; set; }
        [Display(Name = "Address")]
        public string? Address { get; set; }
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = string.Empty;
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; }
        [Display(Name = "Donation Date")]
        public DateTime DonationDate { get; set; }
        [Display(Name = "Generated At")]
        public DateTime GeneratedAt { get; set; }
        public bool IsOfflineDonation { get; set; }
        public string? Remarks { get; set; }
    }
}