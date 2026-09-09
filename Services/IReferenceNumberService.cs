namespace FoundationDonationSystem.Services
{
    public interface IReferenceNumberService
    {
        Task<string> GenerateDonationNumberAsync();
        Task<string> GenerateOfflineDonationNumberAsync();
    }
}