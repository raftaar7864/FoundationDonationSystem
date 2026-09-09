using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(
        Roles = UserRole.Administrator + "," +
                UserRole.DonationVerifier)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(
            ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // DASHBOARD
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var tomorrow = today.AddDays(1);
            var monthStart =
                new DateTime(
                    now.Year,
                    now.Month,
                    1);
            var nextMonth =
                monthStart.AddMonths(1);
            // -----------------------------------------------------
            // TODAY'S DONATIONS
            // -----------------------------------------------------
            var todayDonationAmount =
                await _context.Donations
                    .AsNoTracking()
                    .Where(x =>
                        x.CreatedAt >= today &&
                        x.CreatedAt < tomorrow)
                    .SumAsync(x => (decimal?)x.Amount)
                    ?? 0m;
            // -----------------------------------------------------
            // THIS MONTH
            // -----------------------------------------------------
            var monthDonationAmount =
                await _context.Donations
                    .AsNoTracking()
                    .Where(x =>
                        x.CreatedAt >= monthStart &&
                        x.CreatedAt < nextMonth)
                    .SumAsync(x => (decimal?)x.Amount)
                    ?? 0m;
            // -----------------------------------------------------
            // VERIFIED DONATIONS
            // -----------------------------------------------------
            var verifiedDonationCount =
                await _context.Donations
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.Status == DonationStatus.Verified);
            // -----------------------------------------------------
            // PENDING DONATIONS
            // -----------------------------------------------------
            var pendingDonationCount =
                await _context.Donations
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.Status == DonationStatus.Pending);
            // -----------------------------------------------------
            // TOTAL DONATIONS
            // -----------------------------------------------------
            var totalDonationCount =
                await _context.Donations
                    .AsNoTracking()
                    .CountAsync();
            // -----------------------------------------------------
            // LATEST DONATION
            // -----------------------------------------------------
            var latestDonation =
                await _context.Donations
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefaultAsync();
            // -----------------------------------------------------
            // RECENT DONATIONS
            // -----------------------------------------------------
            var recentDonations =
                await _context.Donations
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id)
                    .Take(5)
                    .ToListAsync();
            var model = new DashboardViewModel
            {
                TodayDonationAmount =
                    todayDonationAmount,
                MonthDonationAmount =
                    monthDonationAmount,
                VerifiedDonationCount =
                    verifiedDonationCount,
                PendingDonationCount =
                    pendingDonationCount,
                TotalDonationCount =
                    totalDonationCount,
                LatestDonationId =
                    latestDonation?.Id ?? 0,
                LatestDonationNumber =
                    latestDonation?.DonationNumber,
                LatestDonorName =
                    latestDonation?.DonorName,
                LatestDonationAmount =
                    latestDonation?.Amount ?? 0m,
                LatestDonationDate =
                    latestDonation?.CreatedAt,
                RecentDonations =
                    recentDonations
            };
            return View(model);
        }
        // =========================================================
        // CHECK FOR NEW DONATIONS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> CheckNewDonations(
            int latestDonationId)
        {
            var newDonation =
                await _context.Donations
                    .AsNoTracking()
                    .Where(x => x.Id > latestDonationId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.DonationNumber,
                        x.DonorName,
                        x.Amount,
                        x.CreatedAt
                    })
                    .FirstOrDefaultAsync();
            if (newDonation == null)
            {
                return Json(new
                {
                    hasNewDonation = false
                });
            }
            return Json(new
            {
                hasNewDonation = true,
                donationId = newDonation.Id,
                donationNumber = newDonation.DonationNumber,
                donorName = newDonation.DonorName,
                amount = newDonation.Amount,
                createdAt = newDonation.CreatedAt
            });
        }
    }
}