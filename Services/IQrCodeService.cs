namespace FoundationDonationSystem.Services
{
    public interface IQrCodeService
    {
        Task<string> SaveAsync(IFormFile file);
        Task DeleteAsync(string? relativePath);
    }
}