using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.Services;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Controllers
{
    [AllowAnonymous]
    public class DonationController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly ApplicationDbContext _context;
        private readonly IFoundationSettingsService _foundationSettingsService;
        public DonationController(
            IDonationService donationService,
            ApplicationDbContext context,
            IFoundationSettingsService foundationSettingsService)
        {
            _donationService = donationService;
            _context = context;
            _foundationSettingsService = foundationSettingsService;
        }
        // =================================================
        // DONATE GET
        // =================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadDonationPageDataAsync();
            var model =
                new DonationCreateViewModel
                {
                    TransactionDate = DateTime.Today
                };
            return View(model);
        }
        // =================================================
        // DONATE POST
        // =================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Index(
            DonationCreateViewModel model)
        {
            // =================================================
            // LOAD PAYMENT ACCOUNTS
            // =================================================
            await LoadDonationPageDataAsync();
            // =================================================
            // NORMALIZE TRANSACTION ID
            // =================================================
            var transactionId =
                model.TransactionId?.Trim() ?? string.Empty;
            model.TransactionId = transactionId;
            // =================================================
            // PAYMENT ACCOUNT VALIDATION
            // =================================================
            if (!await _context.PaymentAccounts.AnyAsync(
                    x => x.IsActive))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Donation payment details are currently unavailable. Please try again later.");
            }
            // =================================================
            // PAYMENT METHOD VALIDATION
            // =================================================
            if (!Enum.IsDefined(
                    typeof(PaymentMethod),
                    model.PaymentMethod))
            {
                ModelState.AddModelError(
                    nameof(model.PaymentMethod),
                    "Please select a valid payment method.");
            }
            // =================================================
            // CONFIRMATION CHECKBOX
            // =================================================
            if (!model.ConfirmDetails)
            {
                ModelState.AddModelError(
                    nameof(model.ConfirmDetails),
                    "Please confirm that the payment details are correct.");
            }
            // =================================================
            // DATE VALIDATION
            // =================================================
            if (model.TransactionDate.Date >
                DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.TransactionDate),
                    "Transaction date cannot be in the future.");
            }
            // =================================================
            // TRANSACTION ID FORMAT VALIDATION
            // =================================================
            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(
                        transactionId,
                        @"^[A-Za-z0-9]+$"))
                {
                    ModelState.AddModelError(
                        nameof(model.TransactionId),
                        "Transaction ID / UTR can contain only letters and numbers. No spaces or special characters are allowed.");
                }
            }
            // =================================================
            // DUPLICATE TRANSACTION ID / UTR CHECK
            // =================================================
            if (!string.IsNullOrWhiteSpace(transactionId) &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    transactionId,
                    @"^[A-Za-z0-9]+$"))
            {
                var normalizedTransactionId =
                    transactionId.ToUpperInvariant();
                var existingDonation =
                    await _context.Donations
                        .AsNoTracking()
                        .Where(x =>
                            !x.IsOfflineDonation &&
                            x.TransactionId != null &&
                            x.TransactionId.ToUpper() ==
                                normalizedTransactionId)
                        .Select(x => new
                        {
                            x.DonationNumber
                        })
                        .FirstOrDefaultAsync();
                if (existingDonation != null)
                {
                    ModelState.AddModelError(
                        nameof(model.TransactionId),
                        "This Transaction ID / UTR has already been submitted.");
                    ViewBag.DuplicateTransaction = true;
                    ViewBag.DuplicateDonationNumber =
                        existingDonation.DonationNumber;
                }
            }
            // =================================================
            // MODEL VALIDATION
            // =================================================
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // =================================================
            // CREATE DONATION
            // =================================================
            try
            {
                var donation =
                    new Donation
                    {
                        DonorName =
                            model.DonorName.Trim(),
                        Mobile =
                            model.Mobile.Trim(),
                        Email =
                            string.IsNullOrWhiteSpace(
                                model.Email)
                                ? null
                                : model.Email.Trim(),
                        Address =
                            string.IsNullOrWhiteSpace(
                                model.Address)
                                ? null
                                : model.Address.Trim(),
                        Amount =
                            model.Amount,
                        PaymentMethod =
                            model.PaymentMethod,
                        TransactionId =
                            transactionId,
                        TransactionDate =
                            model.TransactionDate.Date
                    };
                var created =
                    await _donationService.CreateAsync(
                        donation,
                        model.PaymentProof!);
                // =================================================
                // SUCCESS
                // =================================================
                return RedirectToAction(
                    nameof(Success),
                    new
                    {
                        donationNumber =
                            created.DonationNumber
                    });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    nameof(model.PaymentProof),
                    ex.Message);
                return View(model);
            }
        }
        // =================================================
        // SUCCESS
        // =================================================
        [HttpGet]
        public async Task<IActionResult> Success(
            string donationNumber)
        {
            if (string.IsNullOrWhiteSpace(
                    donationNumber))
            {
                return RedirectToAction(
                    nameof(Index));
            }
            var donation =
                await _donationService
                    .GetByDonationNumberAsync(
                        donationNumber);
            if (donation == null)
            {
                return NotFound();
            }
            return View(donation);
        }
        // =================================================
        // LOAD PUBLIC DONATION PAGE DATA
        // =================================================
        private async Task LoadDonationPageDataAsync()
        {
            var paymentAccounts =
                await _context.PaymentAccounts
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            ViewBag.PaymentAccounts =
                paymentAccounts;
            ViewBag.FoundationSettings =
                await _foundationSettingsService.GetAsync();
        }
    }
}