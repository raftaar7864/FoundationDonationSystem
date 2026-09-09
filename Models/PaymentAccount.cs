namespace FoundationDonationSystem.Models
{
    public class PaymentAccount
    {
        public int Id { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string? Branch { get; set; }
        public string? UpiId { get; set; }
        public string? QrCodePath { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}