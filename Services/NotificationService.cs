using FoundationDonationSystem.Data;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        public NotificationService(
            ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // CHECK FOR NEW DONATION
        // =========================================================
        public async Task<NewDonationNotification?>
            CheckNewDonationAsync(
                int latestDonationId)
        {
            var donation =
                await _context.Donations
                    .AsNoTracking()
                    .Where(x =>
                        x.Id > latestDonationId)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new NewDonationNotification
                    {
                        DonationId = x.Id,
                        DonationNumber =
                            x.DonationNumber,
                        DonorName =
                            x.DonorName,
                        Amount =
                            x.Amount,
                        CreatedAt =
                            x.CreatedAt
                    })
                    .FirstOrDefaultAsync();
            return donation;
        }
        // =========================================================
        // GET LATEST DONATION
        // =========================================================
        public async Task<NewDonationNotification?>
            GetLatestDonationAsync()
        {
            var donation =
                await _context.Donations
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .Select(x => new NewDonationNotification
                    {
                        DonationId = x.Id,
                        DonationNumber =
                            x.DonationNumber,
                        DonorName =
                            x.DonorName,
                        Amount =
                            x.Amount,
                        CreatedAt =
                            x.CreatedAt
                    })
                    .FirstOrDefaultAsync();
            return donation;
        }
    }
}