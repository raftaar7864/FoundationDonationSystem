using FoundationDonationSystem.Models.Enums;
namespace FoundationDonationSystem.ViewModels
{
    public class DonationDetailsViewModel
    {
        public int Id { get; set; }
        public string DonationNumber { get; set; }
            = string.Empty;
        public string DonorName { get; set; }
            = string.Empty;
        public string Mobile { get; set; }
            = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; }
            = string.Empty;
        public DateTime TransactionDate { get; set; }
        public DonationStatus Status { get; set; }
        public string? VerificationRemarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PaymentProofUrl { get; set; }
        public string? DonationSlipNumber { get; set; }
    }
}