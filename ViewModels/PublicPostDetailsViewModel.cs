namespace FoundationDonationSystem.ViewModels
{
    public class PublicPostDetailsViewModel
    {
        // =========================================================
        // POST
        // =========================================================
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Content { get; set; }
        public DateTime PostDate { get; set; }
        // =========================================================
        // FEATURED / COVER IMAGE
        // =========================================================
        public string? FeaturedImagePath { get; set; }
        // =========================================================
        // GALLERY IMAGES
        // Maximum 3 additional photos
        // =========================================================
        public string? GalleryImage1Path { get; set; }
        public string? GalleryImage2Path { get; set; }
        public string? GalleryImage3Path { get; set; }
    }
}