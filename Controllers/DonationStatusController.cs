using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.Services;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Controllers
{
    [AllowAnonymous]
    public class DonationStatusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDonationSlipService _donationSlipService;
        public DonationStatusController(
            ApplicationDbContext context,
            IDonationSlipService donationSlipService)
        {
            _context = context;
            _donationSlipService = donationSlipService;
        }
        // =========================================================
        // CHECK DONATION STATUS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(string? donationNumber)
        {
            if (string.IsNullOrWhiteSpace(donationNumber))
                return View(new DonationStatusViewModel());
            var referenceNumber = donationNumber.Trim();
            var donation = await _context.Donations
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.DonationNumber == referenceNumber);
            if (donation == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Donation reference was not found.");
                return View(new DonationStatusViewModel
                {
                    DonationNumber = referenceNumber
                });
            }
            return View(new DonationStatusViewModel
            {
                DonationNumber = donation.DonationNumber,
                DonorName = donation.DonorName,
                Amount = donation.Amount,
                Status = donation.Status,
                TransactionDate = donation.TransactionDate,
                DonationSlipNumber = donation.DonationSlipNumber,
                VerificationRemarks = donation.VerificationRemarks
            });
        }
        // =========================================================
        // DOWNLOAD DONATION SLIP
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> DownloadSlip(
            string? donationNumber)
        {
            // -----------------------------------------------------
            // Validate donation number
            // -----------------------------------------------------
            if (string.IsNullOrWhiteSpace(donationNumber))
            {
                return NotFound();
            }
            var referenceNumber = donationNumber.Trim();
            // -----------------------------------------------------
            // Find donation
            // -----------------------------------------------------
            var donation = await _context.Donations
                .FirstOrDefaultAsync(x =>
                    x.DonationNumber == referenceNumber);
            if (donation == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // SECURITY:
            // Slip can ONLY be downloaded after verification
            // -----------------------------------------------------
            if (donation.Status != DonationStatus.Verified)
            {
                return Forbid();
            }
            // -----------------------------------------------------
            // Get existing slip
            // -----------------------------------------------------
            var slip = await _donationSlipService
                .GetByDonationIdAsync(donation.Id);
            // -----------------------------------------------------
            // Generate slip if it does not exist
            // -----------------------------------------------------
            if (slip == null)
            {
                slip = await _donationSlipService
                    .GenerateAsync(donation.Id);
            }
            if (slip == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // Generate PDF
            // -----------------------------------------------------
            var pdf = _donationSlipService
                .GeneratePdf(donation, slip);
            // -----------------------------------------------------
            // Download PDF
            // -----------------------------------------------------
            return File(
                pdf,
                "application/pdf",
                $"{slip.SlipNumber}.pdf");
        }
    }
}