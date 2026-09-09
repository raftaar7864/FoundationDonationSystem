using System.ComponentModel.DataAnnotations;
using FoundationDonationSystem.Models.Enums;
namespace FoundationDonationSystem.ViewModels
{
    public class UserCreateViewModel
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        [StringLength(150)]
        [Display(Name = "Designation")]
        public string? Designation { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        [Phone]
        [StringLength(30)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "Password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } =
            UserRole.DonationVerifier;
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}