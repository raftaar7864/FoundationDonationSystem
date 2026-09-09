namespace FoundationDonationSystem.ViewModels
{
    public class PostListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? FeaturedImagePath { get; set; }
        // CATEGORY
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        // SUBCATEGORY
        public int? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public DateTime PostDate { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}