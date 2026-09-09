using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.Services;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ClosedXML.Excel;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    // =========================================================
    // REJECT REQUEST MODEL
    // =========================================================
    public class RejectDonationRequest
    {
        public string? Remarks { get; set; }
    }
    // =========================================================
    // ADMIN DONATIONS CONTROLLER
    // =========================================================
    [Area("Admin")]
    [Authorize(
        Roles = UserRole.Administrator + "," +
                UserRole.DonationVerifier)]
    public class DonationsController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IFileUploadService _fileUploadService;
        private readonly IDonationSlipService _donationSlipService;
        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public DonationsController(
            IDonationService donationService,
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IFileUploadService fileUploadService,
            IDonationSlipService donationSlipService)
        {
            _donationService = donationService;
            _context = context;
            _environment = environment;
            _fileUploadService = fileUploadService;
            _donationSlipService = donationSlipService;
        }
        // =========================================================
        // INDEX
        // PAGINATION + FILTER
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            DonationStatus? status,
            PaymentMethod? paymentMethod,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1,
            int pageSize = 10)
        {
            // -----------------------------------------------------
            // PAGE SIZE
            // -----------------------------------------------------
            var allowedPageSizes =
                new[] { 10, 20, 50 };
            if (!allowedPageSizes.Contains(pageSize))
            {
                pageSize = 10;
            }
            if (page < 1)
            {
                page = 1;
            }
            // -----------------------------------------------------
            // QUERY
            // -----------------------------------------------------
            IQueryable<Donation> query =
                _context.Donations
                    .AsNoTracking();
            // -----------------------------------------------------
            // SEARCH
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query =
                    query.Where(x =>
                        x.DonationNumber.Contains(search) ||
                        x.DonorName.Contains(search) ||
                        x.Mobile.Contains(search) ||
                        x.TransactionId.Contains(search));
            }
            // -----------------------------------------------------
            // STATUS FILTER
            // -----------------------------------------------------
            if (status.HasValue)
            {
                query =
                    query.Where(x =>
                        x.Status == status.Value);
            }
            // -----------------------------------------------------
            // PAYMENT METHOD FILTER
            // -----------------------------------------------------
            if (paymentMethod.HasValue)
            {
                query =
                    query.Where(x =>
                        x.PaymentMethod ==
                        paymentMethod.Value);
            }
            // -----------------------------------------------------
            // DATE FROM
            // -----------------------------------------------------
            if (dateFrom.HasValue)
            {
                var fromDate =
                    dateFrom.Value.Date;
                query =
                    query.Where(x =>
                        x.CreatedAt >= fromDate);
            }
            // -----------------------------------------------------
            // DATE TO
            // -----------------------------------------------------
            if (dateTo.HasValue)
            {
                var toDate =
                    dateTo.Value.Date.AddDays(1);
                query =
                    query.Where(x =>
                        x.CreatedAt < toDate);
            }
            // -----------------------------------------------------
            // TOTAL ITEMS
            // -----------------------------------------------------
            var totalItems =
                await query.CountAsync();
            // -----------------------------------------------------
            // TOTAL PAGES
            // -----------------------------------------------------
            var totalPages =
                (int)Math.Ceiling(
                    totalItems /
                    (double)pageSize);
            // -----------------------------------------------------
            // FIX PAGE
            // -----------------------------------------------------
            if (totalPages > 0 &&
                page > totalPages)
            {
                page = totalPages;
            }
            // -----------------------------------------------------
            // CURRENT PAGE
            // -----------------------------------------------------
            var donations =
                await query
                    .OrderByDescending(
                        x => x.CreatedAt)
                    .Skip(
                        (page - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            // -----------------------------------------------------
            // VIEWBAG
            // -----------------------------------------------------
            ViewBag.Search =
                search;
            ViewBag.Status =
                status;
            ViewBag.PaymentMethod =
                paymentMethod;
            ViewBag.DateFrom =
                dateFrom?.ToString(
                    "yyyy-MM-dd");
            ViewBag.DateTo =
                dateTo?.ToString(
                    "yyyy-MM-dd");
            ViewBag.CurrentPage =
                page;
            ViewBag.PageSize =
                pageSize;
            ViewBag.TotalPages =
                totalPages;
            ViewBag.TotalItems =
                totalItems;
            return View(donations);
        }
        // =========================================================
        // DETAILS POPUP
        // LOAD DONATION + EXISTING SLIP INFORMATION
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // -----------------------------------------------------
            // LOAD DONATION
            // -----------------------------------------------------
            var donation =
                await _context.Donations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (donation == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // LOAD EXISTING DONATION SLIP
            // -----------------------------------------------------
            var slip =
                await _context.DonationSlips
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.DonationId == donation.Id);
            // -----------------------------------------------------
            // IMPORTANT
            //
            // If a slip already exists, make sure the Donation
            // properties used by _DetailsPartial contain the
            // current slip information.
            // -----------------------------------------------------
            if (slip != null)
            {
                donation.DonationSlipNumber =
                    slip.SlipNumber;
                donation.SlipGeneratedAt =
                    slip.GeneratedAt;
            }
            else
            {
                donation.DonationSlipNumber = null;
                donation.SlipGeneratedAt = null;
            }
            // -----------------------------------------------------
            // RETURN DETAILS PARTIAL
            // -----------------------------------------------------
            return PartialView(
                "_DetailsPartial",
                donation);
        }
        // =========================================================
        // VERIFY DONATION
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(
            int id)
        {
            try
            {
                // -------------------------------------------------
                // LOAD DONATION
                // -------------------------------------------------
                var donation =
                    await _context.Donations
                        .FirstOrDefaultAsync(
                            x => x.Id == id);
                if (donation == null)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Donation not found."
                    });
                }
                // -------------------------------------------------
                // CHECK STATUS
                // -------------------------------------------------
                if (donation.Status !=
                    DonationStatus.Pending)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Only pending donations can be verified."
                    });
                }
                // -------------------------------------------------
                // VERIFY
                // -------------------------------------------------
                donation.Status =
                    DonationStatus.Verified;
                donation.VerificationRemarks =
                    "Donation verified.";
                donation.VerifiedByUserId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);
                donation.VerifiedAt =
                    DateTime.UtcNow;
                donation.UpdatedAt =
                    DateTime.UtcNow;
                // -------------------------------------------------
                // SAVE
                // -------------------------------------------------
                await _context.SaveChangesAsync();
                // -------------------------------------------------
                // SUCCESS
                // -------------------------------------------------
                return Json(new
                {
                    success = true,
                    message =
                        "Donation verified successfully."
                });
            }
            catch (DbUpdateException ex)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Database error: " +
                        (ex.InnerException?.Message ??
                         ex.Message)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message =
                        ex.Message
                });
            }
        }
        // =========================================================
        // GENERATE DONATION SLIP
        // IMPORTANT:
        // This action uses NORMAL FORM POST.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSlip(
            int donationId)
        {
            try
            {
                // -------------------------------------------------
                // LOAD DONATION
                // -------------------------------------------------
                var donation =
                    await _context.Donations
                        .FirstOrDefaultAsync(
                            x => x.Id == donationId);
                if (donation == null)
                {
                    TempData["Error"] =
                        "Donation not found.";
                    return RedirectToAction(
                        nameof(Index));
                }
                // -------------------------------------------------
                // ONLY VERIFIED DONATIONS
                // -------------------------------------------------
                if (donation.Status !=
                    DonationStatus.Verified)
                {
                    TempData["Error"] =
                        "Donation must be verified before generating a slip.";
                    return RedirectToAction(
                        nameof(Index));
                }
                // -------------------------------------------------
                // GENERATE / GET EXISTING SLIP
                // -------------------------------------------------
                var slip =
                    await _donationSlipService
                        .GenerateAsync(
                            donationId);
                // -------------------------------------------------
                // SUCCESS
                // -------------------------------------------------
                TempData["Success"] =
                    $"Donation slip {slip.SlipNumber} generated successfully.";
                return RedirectToAction(
                    nameof(ViewSlip),
                    new
                    {
                        id = donationId
                    });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] =
                    ex.Message;
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = donationId
                    });
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] =
                    "Database error: " +
                    (ex.InnerException?.Message ??
                     ex.Message);
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = donationId
                    });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to generate donation slip: " +
                    ex.Message;
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = donationId
                    });
            }
        }
        // =========================================================
        // VIEW DONATION SLIP
        // IMPORTANT:
        // id = DONATION ID
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ViewSlip(
            int id)
        {
            // -----------------------------------------------------
            // LOAD DONATION
            // -----------------------------------------------------
            var donation =
                await _context.Donations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (donation == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // ONLY VERIFIED DONATIONS
            // -----------------------------------------------------
            if (donation.Status !=
                DonationStatus.Verified)
            {
                return BadRequest(
                    "Only verified donations can have a slip.");
            }
            // -----------------------------------------------------
            // LOAD SLIP BY DONATION ID
            // -----------------------------------------------------
            var slip =
                await _donationSlipService
                    .GetByDonationIdAsync(
                        donation.Id);
            if (slip == null)
            {
                return NotFound(
                    "Donation slip not found.");
            }
            // -----------------------------------------------------
            // CREATE VIEW MODEL
            // -----------------------------------------------------
            var model =
                new DonationSlipViewModel
                {
                    Id =
                        slip.Id,
                    DonationId =
                        donation.Id,
                    SlipNumber =
                        slip.SlipNumber,
                    DonationNumber =
                        donation.DonationNumber,
                    DonorName =
                        donation.DonorName,
                    Mobile =
                        donation.Mobile,
                    Email =
                        donation.Email,
                    Address =
                        donation.Address,
                    Amount =
                        donation.Amount,
                    PaymentMethod =
                        donation.PaymentMethod
                            .ToString(),
                    TransactionId =
                        donation.TransactionId,
                    TransactionDate =
                        donation.TransactionDate,
                    DonationDate =
                        donation.CreatedAt,
                    GeneratedAt =
                        slip.GeneratedAt,
                    IsOfflineDonation =
                        donation.IsOfflineDonation,
                    Remarks =
                        donation.VerificationRemarks
                };
            return View(
                "ViewSlip",
                model);
        }
        // =========================================================
        // DOWNLOAD DONATION SLIP PDF
        // IMPORTANT:
        // id = DONATION ID
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> DownloadSlipPdf(
            int id)
        {
            // -----------------------------------------------------
            // LOAD DONATION
            // -----------------------------------------------------
            var donation =
                await _context.Donations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (donation == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // ONLY VERIFIED DONATIONS
            // -----------------------------------------------------
            if (donation.Status !=
                DonationStatus.Verified)
            {
                return BadRequest(
                    "Only verified donations can generate a PDF slip.");
            }
            // -----------------------------------------------------
            // LOAD SLIP
            // -----------------------------------------------------
            var slip =
                await _donationSlipService
                    .GetByDonationIdAsync(
                        donation.Id);
            if (slip == null)
            {
                return NotFound(
                    "Donation slip not found.");
            }
            // -----------------------------------------------------
            // GENERATE PDF
            // -----------------------------------------------------
            var pdf =
                _donationSlipService
                    .GeneratePdf(
                        donation,
                        slip);
            // -----------------------------------------------------
            // RETURN PDF
            // -----------------------------------------------------
            return File(
                pdf,
                "application/pdf",
                $"{slip.SlipNumber}.pdf");
        }
        // =========================================================
        // COLLECT OFFLINE DONATION - GET
        // =========================================================
        [HttpGet("/Admin/Donations/CollectOffline")]
        public IActionResult CollectOffline()
        {
            var model =
                new OfflineDonationCreateViewModel
                {
                    CollectionDate =
                        DateTime.Today
                };
            return PartialView(
                "_OfflineDonationPartial",
                model);
        }
        // =========================================================
        // COLLECT OFFLINE DONATION - POST
        // =========================================================
        [HttpPost("/Admin/Donations/CollectOffline")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1 * 1024 * 1024)]
        public async Task<IActionResult> CollectOffline(
            OfflineDonationCreateViewModel model)
        {
            // -----------------------------------------------------
            // COLLECTION DATE
            // -----------------------------------------------------
            if (model.CollectionDate.Date >
                DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.CollectionDate),
                    "Collection date cannot be in the future.");
            }
            // -----------------------------------------------------
            // MOBILE NUMBER
            // Exactly 10 digits, starting with 6, 7, 8 or 9.
            // -----------------------------------------------------
            var normalizedMobile =
                (model.Mobile ?? string.Empty).Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                    normalizedMobile,
                    @"^[6-9][0-9]{9}$"))
            {
                ModelState.AddModelError(
                    nameof(model.Mobile),
                    "Please enter a valid 10 digit Indian mobile number starting with 6, 7, 8 or 9.");
            }
            // -----------------------------------------------------
            // OFFLINE VOUCHER
            // -----------------------------------------------------
            if (model.OfflineVoucher == null ||
                model.OfflineVoucher.Length <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.OfflineVoucher),
                    "Offline donation voucher is required.");
            }
            // -----------------------------------------------------
            // VOUCHER SIZE AND EXTENSION
            // -----------------------------------------------------
            if (model.OfflineVoucher != null &&
                model.OfflineVoucher.Length > 1 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.OfflineVoucher),
                    "Offline donation voucher must be less than 1 MB.");
            }
            if (model.OfflineVoucher != null &&
                model.OfflineVoucher.Length > 0)
            {
                var extension =
                    Path.GetExtension(model.OfflineVoucher.FileName)
                        .ToLowerInvariant();
                var allowedExtensions =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase)
                    {
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".webp",
                        ".pdf"
                    };
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(model.OfflineVoucher),
                        "Only JPG, JPEG, PNG, WEBP or PDF voucher files are allowed.");
                }
            }
            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                var errors =
                    ModelState
                        .Where(x => x.Value != null)
                        .SelectMany(
                            x => x.Value!.Errors.Select(
                                error => new
                                {
                                    field = x.Key,
                                    message =
                                        string.IsNullOrWhiteSpace(
                                            error.ErrorMessage)
                                            ? error.Exception?.Message
                                            : error.ErrorMessage
                                }))
                        .ToList();
                return Json(new
                {
                    success = false,
                    message =
                        "Please correct the highlighted fields.",
                    errors
                });
            }
            // -----------------------------------------------------
            // CURRENT USER
            // -----------------------------------------------------
            var adminUserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(
                adminUserId))
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Unable to identify the logged-in user."
                });
            }
            try
            {
                // -------------------------------------------------
                // CREATE DONATION
                // -------------------------------------------------
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
                        TransactionDate =
                            model.CollectionDate.Date,
                        VerificationRemarks =
                            string.IsNullOrWhiteSpace(
                                model.Remarks)
                                ? null
                                : model.Remarks.Trim()
                    };
                // -------------------------------------------------
                // SAVE OFFLINE DONATION
                // -------------------------------------------------
                var created =
                    await _donationService
                        .CreateOfflineAsync(
                            donation,
                            model.OfflineVoucher!,
                            adminUserId);
                // -------------------------------------------------
                // SUCCESS
                // -------------------------------------------------
                return Json(new
                {
                    success = true,
                    message =
                        "Cash donation collected successfully.",
                    donationNumber =
                        created.DonationNumber
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new
                {
                    success = false,
                    message =
                        ex.Message
                });
            }
            catch (DbUpdateException ex)
            {
                var databaseError =
                    ex.InnerException?.Message ??
                    ex.Message;
                return Json(new
                {
                    success = false,
                    message =
                        "Database error: " +
                        databaseError
                });
            }
            catch (Exception ex)
            {
                var error =
                    ex.InnerException?.Message ??
                    ex.Message;
                return Json(new
                {
                    success = false,
                    message =
                        "Unable to save offline donation: " +
                        error
                });
            }
        }
        // =========================================================
        // VIEW OFFLINE DONATION VOUCHER
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> OfflineVoucher(
            int id)
        {
            // -----------------------------------------------------
            // LOAD DONATION
            // -----------------------------------------------------
            var donation =
                await _context.Donations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (donation == null ||
                !donation.IsOfflineDonation ||
                string.IsNullOrWhiteSpace(
                    donation.OfflineVoucherPath))
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // RESOLVE PRIVATE FILE
            // -----------------------------------------------------
            var fullPath =
                ResolvePrivateFilePath(
                    donation.OfflineVoucherPath,
                    "OfflineDonationVouchers");
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // RETURN FILE FOR INLINE PREVIEW
            // -----------------------------------------------------
            var contentType =
                GetFileContentType(fullPath);
            Response.Headers.CacheControl =
                "no-store, no-cache, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            var fileBytes =
                await System.IO.File
                    .ReadAllBytesAsync(fullPath);
            return File(
                fileBytes,
                contentType,
                enableRangeProcessing: true);
        }
        // =========================================================
        // REJECT DONATION
        // IMPORTANT:
        // The donations page submits rejection data using FormData.
        // Therefore the request must use [FromForm], not [FromBody].
        // This works for Administrator and DonationVerifier.
        // =========================================================
        [Area("Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Administrator + "," + UserRole.DonationVerifier)]
        public async Task<IActionResult> Reject(
            int id,
            [FromForm]
            RejectDonationRequest request)
        {
            try
            {
                // -------------------------------------------------
                // LOAD DONATION
                // -------------------------------------------------
                var donation =
                    await _context.Donations
                        .FirstOrDefaultAsync(
                            x => x.Id == id);
                if (donation == null)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Donation not found."
                    });
                }
                // -------------------------------------------------
                // ONLY PENDING DONATIONS
                // -------------------------------------------------
                if (donation.Status !=
                    DonationStatus.Pending)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Only pending donations can be rejected."
                    });
                }
                // -------------------------------------------------
                // GET REMARKS
                // -------------------------------------------------
                var remarks =
                    request?.Remarks?.Trim();
                // -------------------------------------------------
                // REMARKS REQUIRED
                // -------------------------------------------------
                if (string.IsNullOrWhiteSpace(
                    remarks))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Please provide a reason for rejecting the donation."
                    });
                }
                // -------------------------------------------------
                // REMARKS LENGTH
                // -------------------------------------------------
                if (remarks.Length > 500)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Rejection remarks cannot exceed 500 characters."
                    });
                }
                // -------------------------------------------------
                // REJECT
                // -------------------------------------------------
                donation.Status =
                    DonationStatus.Rejected;
                donation.VerificationRemarks =
                    remarks;
                donation.VerifiedByUserId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);
                donation.VerifiedAt =
                    DateTime.UtcNow;
                donation.UpdatedAt =
                    DateTime.UtcNow;
                // -------------------------------------------------
                // SAVE
                // -------------------------------------------------
                await _context.SaveChangesAsync();
                // -------------------------------------------------
                // SUCCESS
                // -------------------------------------------------
                return Json(new
                {
                    success = true,
                    message =
                        "Donation rejected successfully."
                });
            }
            catch (DbUpdateException ex)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Database error: " +
                        (ex.InnerException?.Message ??
                         ex.Message)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message =
                        ex.Message
                });
            }
        }
        // =========================================================
        // DELETE DONATION
        // =========================================================
        [Area("Admin")]
        [Authorize(Roles = UserRole.Administrator)]
        [HttpPost("/Admin/Donations/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            // -----------------------------------------------------
            // LOAD DONATION
            // -----------------------------------------------------
            var donation =
                await _context.Donations
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (donation == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Donation not found."
                });
            }
            try
            {
                // -------------------------------------------------
                // DELETE PAYMENT PROOF FILE
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    donation.PaymentProofPath))
                {
                    await _fileUploadService
                        .DeletePaymentProofAsync(
                            donation.PaymentProofPath);
                }
                // -------------------------------------------------
                // DELETE OFFLINE VOUCHER FILE
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    donation.OfflineVoucherPath))
                {
                    await _fileUploadService
                        .DeleteOfflineVoucherAsync(
                            donation.OfflineVoucherPath);
                }
                // -------------------------------------------------
                // DELETE DONATION SLIP FIRST
                //
                // DonationSlip -> Donation uses
                // DeleteBehavior.Restrict.
                // Therefore the slip must be removed first.
                // -------------------------------------------------
                var donationSlip =
                    await _context.DonationSlips
                        .FirstOrDefaultAsync(
                            x => x.DonationId == id);
                if (donationSlip != null)
                {
                    _context.DonationSlips.Remove(
                        donationSlip);
                }
                // -------------------------------------------------
                // DELETE DONATION
                // -------------------------------------------------
                _context.Donations.Remove(
                    donation);
                // -------------------------------------------------
                // SAVE
                // -------------------------------------------------
                await _context.SaveChangesAsync();
                // -------------------------------------------------
                // SUCCESS
                // -------------------------------------------------
                return Json(new
                {
                    success = true,
                    message =
                        "Donation deleted successfully."
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message =
                            "Database error: " +
                            (ex.InnerException?.Message ??
                             ex.Message)
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message =
                            "Unable to delete donation: " +
                            ex.Message
                    });
            }
        }
        // =========================================================
        // PRIVATE FILE RESOLVER
        // =========================================================
        // Supports both:
        //   ContentRoot/PrivateUploads/...
        //   wwwroot/PrivateUploads/...
        //
        // The stored database value is always reduced to a safe
        // file name before it is combined with the application path.
        // =========================================================
        private string? ResolvePrivateFilePath(
            string? storedPath,
            string folderName)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
            {
                return null;
            }
            var safeFileName =
                Path.GetFileName(storedPath);
            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                return null;
            }
            var candidates = new[]
            {
                Path.Combine(
                    _environment.ContentRootPath,
                    "PrivateUploads",
                    folderName,
                    safeFileName),
                Path.Combine(
                    _environment.WebRootPath ?? string.Empty,
                    "PrivateUploads",
                    folderName,
                    safeFileName)
            };
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) &&
                    System.IO.File.Exists(candidate))
                {
                    return candidate;
                }
            }
            return null;
        }
        // =========================================================
        // CONTENT TYPE
        // =========================================================
        private static string GetFileContentType(
            string filePath)
        {
            var extension =
                Path.GetExtension(filePath)
                    .ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
        // =========================================================
        // SECURE PAYMENT PROOF
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> PaymentProof(
            int id)
        {
            // -----------------------------------------------------
            // LOAD DONATION
            // -----------------------------------------------------
            var donation =
                await _context.Donations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (donation == null ||
                string.IsNullOrWhiteSpace(
                    donation.PaymentProofPath))
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // RESOLVE PRIVATE FILE
            // -----------------------------------------------------
            var fullPath =
                ResolvePrivateFilePath(
                    donation.PaymentProofPath,
                    "DonationProofs");
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // RETURN FILE FOR INLINE PREVIEW
            // -----------------------------------------------------
            var contentType =
                GetFileContentType(fullPath);
            Response.Headers.CacheControl =
                "no-store, no-cache, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            var fileBytes =
                await System.IO.File
                    .ReadAllBytesAsync(fullPath);
            return File(
                fileBytes,
                contentType,
                enableRangeProcessing: true);
        }
        // =========================================================
        // EXPORT DONATIONS TO EXCEL
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            string? search,
            DonationStatus? status,
            PaymentMethod? paymentMethod,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var query = _context.Donations
                .AsNoTracking()
                .AsQueryable();
            // =====================================================
            // SEARCH
            // =====================================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    x.DonationNumber.Contains(search) ||
                    x.DonorName.Contains(search) ||
                    x.Mobile.Contains(search) ||
                    (x.TransactionId != null &&
                     x.TransactionId.Contains(search)));
            }
            // =====================================================
            // STATUS
            // =====================================================
            if (status.HasValue)
            {
                query = query.Where(x =>
                    x.Status == status.Value);
            }
            // =====================================================
            // PAYMENT METHOD
            // =====================================================
            if (paymentMethod.HasValue)
            {
                query = query.Where(x =>
                    x.PaymentMethod == paymentMethod.Value);
            }
            // =====================================================
            // DATE FROM
            // =====================================================
            if (dateFrom.HasValue)
            {
                var fromDate = dateFrom.Value.Date;
                query = query.Where(x =>
                    x.CreatedAt >= fromDate);
            }
            // =====================================================
            // DATE TO
            // =====================================================
            if (dateTo.HasValue)
            {
                var toDateExclusive =
                    dateTo.Value.Date.AddDays(1);
                query = query.Where(x =>
                    x.CreatedAt < toDateExclusive);
            }
            // =====================================================
            // LOAD DATA
            // =====================================================
            var donations = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            // =====================================================
            // CREATE EXCEL
            // =====================================================
            using var workbook = new XLWorkbook();
            var worksheet =
                workbook.Worksheets.Add("Donations");
            // =====================================================
            // HEADER
            // =====================================================
            worksheet.Cell(1, 1).Value = "Donation No.";
            worksheet.Cell(1, 2).Value = "Donor Name";
            worksheet.Cell(1, 3).Value = "Mobile";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Amount";
            worksheet.Cell(1, 6).Value = "Payment Method";
            worksheet.Cell(1, 7).Value = "Transaction ID";
            worksheet.Cell(1, 8).Value = "Status";
            worksheet.Cell(1, 9).Value = "Donation Date";
            worksheet.Cell(1, 10).Value = "Created At";
            // =====================================================
            // HEADER STYLE
            // =====================================================
            var headerRange =
                worksheet.Range(1, 1, 1, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#EA580C");
            headerRange.Style.Font.FontColor =
                XLColor.White;
            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
            // =====================================================
            // DATA
            // =====================================================
            var row = 2;
            foreach (var donation in donations)
            {
                worksheet.Cell(row, 1).Value =
                    donation.DonationNumber;
                worksheet.Cell(row, 2).Value =
                    donation.DonorName;
                worksheet.Cell(row, 3).Value =
                    donation.Mobile;
                worksheet.Cell(row, 4).Value =
                    donation.Email ?? string.Empty;
                worksheet.Cell(row, 5).Value =
                    donation.Amount;
                worksheet.Cell(row, 6).Value =
                    donation.PaymentMethod.ToString();
                worksheet.Cell(row, 7).Value =
                    donation.TransactionId ?? string.Empty;
                worksheet.Cell(row, 8).Value =
                    donation.Status.ToString();
                worksheet.Cell(row, 9).Value =
                    donation.TransactionDate;
                worksheet.Cell(row, 10).Value =
                    donation.CreatedAt;
                row++;
            }
            // =====================================================
            // FORMATTING
            // =====================================================
            worksheet.Column(5)
                .Style.NumberFormat.Format = "₹#,##0.00";
            worksheet.Column(9)
                .Style.DateFormat.Format = "dd-MM-yyyy";
            worksheet.Column(10)
                .Style.DateFormat.Format =
                    "dd-MM-yyyy HH:mm";
            worksheet.Columns()
                .AdjustToContents();
            // Prevent excessively wide columns
            for (var column = 1; column <= 10; column++)
            {
                if (worksheet.Column(column).Width > 35)
                {
                    worksheet.Column(column).Width = 35;
                }
            }
            // =====================================================
            // FILTER INFORMATION
            // =====================================================
            worksheet.SheetView.FreezeRows(1);
            worksheet.Range(
                1,
                1,
                Math.Max(row - 1, 1),
                10)
                .SetAutoFilter();
            // =====================================================
            // RETURN FILE
            // =====================================================
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            var fileName =
                $"Donations-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
