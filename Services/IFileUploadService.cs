namespace FoundationDonationSystem.Services
{
    public interface IFileUploadService
    {
        Task<string> SavePaymentProofAsync(
            IFormFile file,
            string donationNumber);
        Task DeletePaymentProofAsync(string? fileName);
        Task<string> SaveOfflineVoucherAsync(
            IFormFile file,
            string donationNumber);
        Task DeleteOfflineVoucherAsync(
            string? filePath);
    }
}