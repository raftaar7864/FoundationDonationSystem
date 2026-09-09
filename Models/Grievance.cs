using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.Models
{
    public class Grievance
    {
        public int Id { get; set; }
        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Mobile { get; set; } = string.Empty;
        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }
        [Required, StringLength(100)]
        public string Category { get; set; } = string.Empty;
        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        [Required, StringLength(5000)]
        public string Description { get; set; } = string.Empty;
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }
        [StringLength(50)]
        public string GrievanceNumber { get; set; } = string.Empty;
        [StringLength(30)]
        public string Status { get; set; } = "Pending";
        [StringLength(4000)]
        public string? AdminRemarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
