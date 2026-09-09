using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(
        Roles = UserRole.Administrator + "," +
                UserRole.DonationVerifier)]
    public class NotificationController : Controller
    {
        private readonly INotificationService
            _notificationService;
        public NotificationController(
            INotificationService notificationService)
        {
            _notificationService =
                notificationService;
        }
        // =========================================================
        // CHECK NEW DONATIONS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult>
            CheckNewDonations(
                int latestDonationId)
        {
            var donation =
                await _notificationService
                    .CheckNewDonationAsync(
                        latestDonationId);
            if (donation == null)
            {
                return Json(new
                {
                    hasNewDonation = false
                });
            }
            return Json(new
            {
                hasNewDonation = true,
                donationId =
                    donation.DonationId,
                donationNumber =
                    donation.DonationNumber,
                donorName =
                    donation.DonorName,
                amount =
                    donation.Amount,
                createdAt =
                    donation.CreatedAt
            });
        }
        // =========================================================
        // GET LATEST DONATION
        // =========================================================
        [HttpGet]
        public async Task<IActionResult>
            LatestDonation()
        {
            var donation =
                await _notificationService
                    .GetLatestDonationAsync();
            if (donation == null)
            {
                return Json(new
                {
                    hasDonation = false
                });
            }
            return Json(new
            {
                hasDonation = true,
                donationId =
                    donation.DonationId,
                donationNumber =
                    donation.DonationNumber,
                donorName =
                    donation.DonorName,
                amount =
                    donation.Amount,
                createdAt =
                    donation.CreatedAt
            });
        }
    }
}