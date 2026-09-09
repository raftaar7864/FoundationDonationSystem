using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.ViewModels
{
    public class PaymentAccountViewModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; } = string.Empty;
        [Required]
        [StringLength(150)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        [Display(Name = "IFSC Code")]
        public string IFSC { get; set; } = string.Empty;
        [StringLength(150)]
        public string? Branch { get; set; }
        [StringLength(150)]
        [Display(Name = "UPI ID")]
        public string? UpiId { get; set; }
        [Display(Name = "QR Code")]
        public IFormFile? QrCode { get; set; }
        public string? ExistingQrCodePath { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}