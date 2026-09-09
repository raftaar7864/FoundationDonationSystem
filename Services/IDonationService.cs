using FoundationDonationSystem.Models;
namespace FoundationDonationSystem.Services
{
    public interface IDonationService
    {
        Task<Donation> CreateAsync(
            Donation donation,
            IFormFile paymentProof);
        Task<Donation> CreateOfflineAsync(
            Donation donation,
            IFormFile offlineVoucher,
            string adminUserId);
        Task<Donation?> GetByIdAsync(int id);
        Task<Donation?> GetByDonationNumberAsync(
            string donationNumber);
        Task<List<Donation>> GetAllAsync();
    }
}