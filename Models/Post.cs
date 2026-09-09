using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.Models
{
    public class Post
    {
        public int Id { get; set; }
        // =========================================================
        // POST CONTENT
        // =========================================================
        [Required]
        [StringLength(
            250,
            MinimumLength = 3,
            ErrorMessage = "Title must be between 3 and 250 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;
        [StringLength(
            500,
            ErrorMessage = "Short description cannot exceed 500 characters.")]
        [Display(Name = "Short Description")]
        public string? ShortDescription { get; set; }
        [Display(Name = "Content")]
        public string? Content { get; set; }
        // =========================================================
        // FEATURED / COVER IMAGE
        // =========================================================
        [StringLength(500)]
        [Display(Name = "Featured Image")]
        public string? FeaturedImagePath { get; set; }
        // =========================================================
        // POST DATE & ORDER
        // =========================================================
        [Display(Name = "Post Date")]
        public DateTime PostDate { get; set; } = DateTime.Today;
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;
        // =========================================================
        // PUBLISHING
        // =========================================================
        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = false;
        // =========================================================
        // CREATED / UPDATED
        // =========================================================
        [Required]
        [StringLength(450)]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        // =========================================================
        // CATEGORY
        // =========================================================
        public int CategoryId { get; set; }
        public PostCategory? Category { get; set; }
        // =========================================================
        // SUBCATEGORY
        // OPTIONAL
        //
        // Activities -> Subcategory required
        // Announcement -> No subcategory
        // News -> No subcategory
        // =========================================================
        public int? SubCategoryId { get; set; }
        public PostSubCategory? SubCategory { get; set; }
        // =========================================================
        // GALLERY IMAGES
        // =========================================================
        public ICollection<PostImage> Images { get; set; }
            = new List<PostImage>();
    }
}