using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.Models
{
    public class PostImage
    {
        public int Id { get; set; }
        // =========================================================
        // POST RELATIONSHIP
        // =========================================================
        [Required]
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
        // =========================================================
        // IMAGE
        // =========================================================
        [Required]
        [StringLength(500)]
        [Display(Name = "Image Path")]
        public string ImagePath { get; set; } = string.Empty;
        // =========================================================
        // DISPLAY ORDER
        // =========================================================
        [Range(1, 3)]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }
        // =========================================================
        // CREATED
        // =========================================================
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}