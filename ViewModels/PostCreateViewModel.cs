using System.ComponentModel.DataAnnotations;
namespace FoundationDonationSystem.ViewModels
{
    public class PostCreateViewModel
    {
        // =========================================================
        // POST CONTENT
        // =========================================================
        [Required(ErrorMessage = "Title is required.")]
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
        [Display(Name = "Featured Image")]
        public IFormFile? FeaturedImage { get; set; }
        // =========================================================
        // GALLERY IMAGES
        // Maximum 3 additional photos
        // =========================================================
        [Display(Name = "Gallery Photo 1")]
        public IFormFile? GalleryImage1 { get; set; }
        [Display(Name = "Gallery Photo 2")]
        public IFormFile? GalleryImage2 { get; set; }
        [Display(Name = "Gallery Photo 3")]
        public IFormFile? GalleryImage3 { get; set; }
        // =========================================================
        // POST DATE
        // =========================================================
        [Required(ErrorMessage = "Post date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Post Date")]
        public DateTime PostDate { get; set; } = DateTime.Today;
        // =========================================================
        // DISPLAY ORDER
        // =========================================================
        [Range(
            0,
            999999,
            ErrorMessage = "Display order must be 0 or greater.")]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;
        // =========================================================
        // PUBLISHING
        // =========================================================
        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = false;
        // =========================================================
        // CATEGORY
        // =========================================================
        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [Display(Name = "Subcategory")]
        public int? SubCategoryId { get; set; }
    }
}