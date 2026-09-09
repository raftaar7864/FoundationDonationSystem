using System.ComponentModel.DataAnnotations;
using FoundationDonationSystem.Models.Enums;
namespace FoundationDonationSystem.ViewModels
{
    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;
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
        [Display(Name = "Role")]
        [Required]
        public string Role { get; set; } =
            UserRole.DonationVerifier;
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "Password must be at least 8 characters.")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage =
                "New password and confirmation password do not match.")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
    }
}