namespace FoundationDonationSystem.Models
{
    public class PostSubCategory
    {
        public int Id { get; set; }
        public int PostCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public PostCategory? PostCategory { get; set; }
    }
}