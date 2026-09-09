using System.ComponentModel.DataAnnotations;
using FoundationDonationSystem.Models.Enums;
namespace FoundationDonationSystem.ViewModels
{
    public class DonationVerificationViewModel
    {
        public int Id { get; set; }
        public string DonationNumber { get; set; } = string.Empty;
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public DonationStatus Status { get; set; }
        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string? VerificationRemarks { get; set; }
    }
}
