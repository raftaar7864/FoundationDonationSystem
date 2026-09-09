namespace FoundationDonationSystem.Models
{
    public class PostCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<PostSubCategory> SubCategories { get; set; }
            = new List<PostSubCategory>();
    }
}