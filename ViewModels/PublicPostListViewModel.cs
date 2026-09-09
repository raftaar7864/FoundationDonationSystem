namespace FoundationDonationSystem.ViewModels
{
    public class PublicPostListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? FeaturedImagePath { get; set; }
        public DateTime PostDate { get; set; }
        // =========================================================
        // CATEGORY
        // =========================================================
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        // =========================================================
        // SUB CATEGORY
        // =========================================================
        public int? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
    }
}