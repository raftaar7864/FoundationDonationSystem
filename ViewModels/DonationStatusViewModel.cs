using FoundationDonationSystem.Models.Enums;
namespace FoundationDonationSystem.ViewModels
{
    public class DonationStatusViewModel
    {
        public string DonationNumber { get; set; } = string.Empty;
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DonationStatus? Status { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? DonationSlipNumber { get; set; }
        public string? VerificationRemarks { get; set; }
    }
}
