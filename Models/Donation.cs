using FoundationDonationSystem.Models.Enums;
namespace FoundationDonationSystem.Models
{
    public class Donation
    {
        public int Id { get; set; }
        public string DonationNumber { get; set; } = string.Empty;
        // =====================================================
        // DONOR INFORMATION
        // =====================================================
        public string DonorName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        // =====================================================
        // DONATION INFORMATION
        // =====================================================
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        // =====================================================
        // ONLINE PAYMENT PROOF
        // =====================================================
        public string? PaymentProofPath { get; set; }
        public string? PaymentProofOriginalName { get; set; }
        // =====================================================
        // OFFLINE DONATION VOUCHER
        // =====================================================
        public string? OfflineVoucherPath { get; set; }
        public string? OfflineVoucherOriginalName { get; set; }
        // =====================================================
        // OFFLINE DONATION INFORMATION
        // =====================================================
        public bool IsOfflineDonation { get; set; }
        // =====================================================
        // VERIFICATION
        // =====================================================
        public DonationStatus Status { get; set; }
            = DonationStatus.Pending;
        public string? VerificationRemarks { get; set; }
        public string? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        // =====================================================
        // DONATION SLIP
        // =====================================================
        public string? DonationSlipNumber { get; set; }
        public DateTime? SlipGeneratedAt { get; set; }
        // =====================================================
        // AUDIT
        // =====================================================
        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}