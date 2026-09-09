namespace FoundationDonationSystem.Models
{
    public class DonationSlip
    {
        public int Id { get; set; }
        public int DonationId { get; set; }
        public string SlipNumber { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
            = DateTime.UtcNow;
        public string? PdfFilePath { get; set; }
        public string GeneratedByUserId { get; set; } = string.Empty;
        public Donation Donation { get; set; } = null!;
    }
}