using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Services
{
    public interface IFoundationSettingsService
    {
        Task<FoundationSetting> GetAsync();
    }
    public class FoundationSettingsService : IFoundationSettingsService
    {
        private readonly ApplicationDbContext _context;
        public FoundationSettingsService(
            ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<FoundationSetting> GetAsync()
        {
            var setting = await _context.FoundationSettings
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
            if (setting != null)
            {
                return setting;
            }
            return new FoundationSetting
            {
                FoundationName = "Foundation",
                Tagline = "Serving with compassion • Supporting with purpose",
                ReceiptFooter =
                    "Thank you for your generous contribution.",
                ThankYouMessage =
                    "Your contribution makes a difference."
            };
        }
    }
}