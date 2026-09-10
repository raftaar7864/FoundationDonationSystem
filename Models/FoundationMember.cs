using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FoundationDonationSystem.Models
{
    public class FoundationMember
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;
        [StringLength(2000)]
        public string? ShortBio { get; set; }
        [StringLength(500)]
        public string? ProfileImage { get; set; }
        [NotMapped]
        public IFormFile? ProfileImageFile { get; set; }
        [NotMapped]
        public bool RemoveProfileImage { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}