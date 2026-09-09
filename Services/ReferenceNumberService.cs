using FoundationDonationSystem.Data;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Services
{
    public class ReferenceNumberService : IReferenceNumberService
    {
        private readonly ApplicationDbContext _context;
        public ReferenceNumberService(
            ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // ONLINE DONATION NUMBER
        // Format:
        // ICF-2026-4837192
        // =========================================================
        public async Task<string> GenerateDonationNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            const int maxAttempts = 100;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int randomNumber =
                    Random.Shared.Next(
                        1_000_000,
                        10_000_000);
                string donationNumber =
                    "ICF-" +
                    currentYear +
                    "-" +
                    randomNumber.ToString("D5");
                bool exists =
                    await _context.Donations
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.DonationNumber ==
                            donationNumber);
                if (!exists)
                {
                    return donationNumber;
                }
            }
            throw new InvalidOperationException(
                "Unable to generate a unique donation number. Please try again.");
        }
        // =========================================================
        // OFFLINE DONATION NUMBER
        // Format:
        // ICF-2026-O583921
        // =========================================================
        public async Task<string> GenerateOfflineDonationNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            const int maxAttempts = 100;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int randomNumber =
                    Random.Shared.Next(
                        0,
                        1_000_000);
                string randomPart =
                    randomNumber.ToString("D4");
                string donationNumber =
                    "ICF-" +
                    currentYear +
                    "-O" +
                    randomPart;
                bool exists =
                    await _context.Donations
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.DonationNumber ==
                            donationNumber);
                if (!exists)
                {
                    return donationNumber;
                }
            }
            throw new InvalidOperationException(
                "Unable to generate a unique offline donation number. Please try again.");
        }
    }
}