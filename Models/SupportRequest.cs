using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.Models
{
    public class SupportRequest
    {
        public int Id { get; set; }
        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Mobile { get; set; } = string.Empty;
        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }
        [StringLength(300)]
        public string? Address { get; set; }
        [Required, StringLength(100)]
        public string SupportCategory { get; set; } = string.Empty;
        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        [Required, StringLength(4000)]
        public string Description { get; set; } = string.Empty;
        [StringLength(30)]
        public string PreferredContactMethod { get; set; } = "Phone";
        [StringLength(50)]
        public string RequestNumber { get; set; } = string.Empty;
        [StringLength(30)]
        public string Status { get; set; } = "Pending";
        [StringLength(4000)]
        public string? AdminRemarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
