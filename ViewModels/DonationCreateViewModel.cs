using FoundationDonationSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.ViewModels
{
    public class DonationCreateViewModel
    {
        // =================================================
        // DONOR INFORMATION
        // =================================================
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(
            150,
            MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 150 characters.")]
        [RegularExpression(
            @"^[A-Za-z]+(?: [A-Za-z]+)*$",
            ErrorMessage = "Name can contain only letters and single spaces between words.")]
        [Display(Name = "Full Name")]
        public string DonorName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(
            @"^[6-9][0-9]{9}$",
            ErrorMessage = "Enter a valid 10-digit mobile number starting with 6, 7, 8 or 9.")]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(
            150,
            ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }
        [StringLength(
            500,
            ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }
        // =================================================
        // DONATION
        // =================================================
        [Required(ErrorMessage = "Donation amount is required.")]
        [Range(
            typeof(decimal),
            "1",
            "999999999",
            ErrorMessage = "Donation amount must be between ₹1 and ₹999,999,999.")]
        [Display(Name = "Donation Amount")]
        public decimal Amount { get; set; }
        // =================================================
        // PAYMENT INFORMATION
        // =================================================
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }
        [Required(ErrorMessage = "Transaction ID / UTR is required.")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Transaction ID / UTR must be between 3 and 100 characters.")]
        [RegularExpression(
            @"^[A-Za-z0-9]+$",
            ErrorMessage = "Transaction ID / UTR can contain only letters and numbers. No spaces or special characters are allowed.")]
        [Display(Name = "Transaction ID / UTR *")]
        public string TransactionId { get; set; } = string.Empty;
        [Required(ErrorMessage = "Transaction date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; }
            = DateTime.Today;
        [Required(ErrorMessage = "Payment proof is required.")]
        [Display(Name = "Payment Proof")]
        public IFormFile? PaymentProof { get; set; }
        // =================================================
        // CONFIRMATION
        // =================================================
        [Required(ErrorMessage = "Please confirm that the submitted details are correct.")]
        [Display(Name = "Confirmation")]
        public bool ConfirmDetails { get; set; }
    }
}