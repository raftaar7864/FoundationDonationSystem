using FoundationDonationSystem.Models;
namespace FoundationDonationSystem.Services
{
    public interface IDonationSlipService
    {
        Task<DonationSlip?> GetByIdAsync(int id);
        Task<DonationSlip?> GetByDonationIdAsync(
            int donationId);
        Task<DonationSlip> GenerateAsync(
            int donationId);
        Task<List<DonationSlip>> GetAllAsync();
        byte[] GeneratePdf(
            Donation donation,
            DonationSlip slip);
    }
}