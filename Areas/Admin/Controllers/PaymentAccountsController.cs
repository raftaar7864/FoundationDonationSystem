using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.Services;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRole.Administrator)]
    public class PaymentAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IQrCodeService _qrCodeService;
        public PaymentAccountsController(
            ApplicationDbContext context,
            IQrCodeService qrCodeService)
        {
            _context = context;
            _qrCodeService = qrCodeService;
        }
        // =========================================================
        // INDEX
        // GET: /admin/paymentaccounts
        // =========================================================
        [HttpGet]
        [Route("/Admin/PaymentAccounts")]
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.PaymentAccounts
                .AsNoTracking()
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.AccountName)
                .ToListAsync();
            return View(accounts);
        }
        // =========================================================
        // CREATE - GET
        // AJAX POPUP
        //
        // GET: /admin/paymentaccounts/create
        // =========================================================
        [HttpGet]
        [Route("/Admin/PaymentAccounts/Create")]
        public IActionResult Create()
        {
            var model = new PaymentAccountViewModel
            {
                IsActive = true
            };
            return PartialView(
                "_CreatePartial",
                model);
        }
        // =========================================================
        // CREATE - POST
        // AJAX POPUP
        //
        // POST: /admin/paymentaccounts/create
        // =========================================================
        [HttpPost]
        [Route("/Admin/PaymentAccounts/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PaymentAccountViewModel model)
        {
            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            string? qrCodePath = null;
            try
            {
                // -------------------------------------------------
                // SAVE QR CODE
                // -------------------------------------------------
                if (model.QrCode != null &&
                    model.QrCode.Length > 0)
                {
                    qrCodePath =
                        await _qrCodeService.SaveAsync(
                            model.QrCode);
                }
                // -------------------------------------------------
                // CREATE ACCOUNT
                // -------------------------------------------------
                var account = new PaymentAccount
                {
                    AccountName =
                        model.AccountName.Trim(),
                    BankName =
                        model.BankName.Trim(),
                    AccountNumber =
                        model.AccountNumber.Trim(),
                    IFSC =
                        model.IFSC
                            .Trim()
                            .ToUpperInvariant(),
                    Branch =
                        string.IsNullOrWhiteSpace(
                            model.Branch)
                            ? null
                            : model.Branch.Trim(),
                    UpiId =
                        string.IsNullOrWhiteSpace(
                            model.UpiId)
                            ? null
                            : model.UpiId.Trim(),
                    QrCodePath =
                        qrCodePath,
                    IsActive =
                        model.IsActive,
                    CreatedAt =
                        DateTime.UtcNow
                };
                // -------------------------------------------------
                // ADD TO DATABASE
                // -------------------------------------------------
                _context.PaymentAccounts.Add(
                    account);
                await _context.SaveChangesAsync();
                // -------------------------------------------------
                // SUCCESS MESSAGE
                // -------------------------------------------------
                TempData["SuccessMessage"] =
                    "Payment account created successfully.";
                // -------------------------------------------------
                // AJAX SUCCESS
                // -------------------------------------------------
                return Json(new
                {
                    success = true,
                    message =
                        "Payment account created successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                // -------------------------------------------------
                // REMOVE UPLOADED QR IF OPERATION FAILED
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    qrCodePath))
                {
                    await _qrCodeService.DeleteAsync(
                        qrCodePath);
                }
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            catch (Exception ex)
            {
                // -------------------------------------------------
                // REMOVE UPLOADED QR IF OPERATION FAILED
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    qrCodePath))
                {
                    await _qrCodeService.DeleteAsync(
                        qrCodePath);
                }
                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to create payment account: {ex.Message}");
                return PartialView(
                    "_CreatePartial",
                    model);
            }
        }
        // =========================================================
        // DETAILS / VIEW
        // AJAX POPUP
        //
        // GET: /admin/paymentaccounts/details/5
        // =========================================================
        [HttpGet]
        [Route("/Admin/PaymentAccounts/Details/{id:int}")]
        public async Task<IActionResult> Details(
            int id)
        {
            var account =
                await _context.PaymentAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Payment account not found."
                });
            }
            // -----------------------------------------------------
            // RETURN VIEW PARTIAL
            // -----------------------------------------------------
            return PartialView(
                "_DetailsPartial",
                account);
        }
        // =========================================================
        // EDIT - GET
        // AJAX POPUP
        //
        // GET: /admin/paymentaccounts/edit/5
        // =========================================================
        [HttpGet]
        [Route("/Admin/PaymentAccounts/Edit/{id:int}")]
        public async Task<IActionResult> Edit(
            int id)
        {
            var account =
                await _context.PaymentAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Payment account not found."
                });
            }
            // -----------------------------------------------------
            // CREATE VIEW MODEL
            // -----------------------------------------------------
            var model =
                new PaymentAccountViewModel
                {
                    Id =
                        account.Id,
                    AccountName =
                        account.AccountName,
                    BankName =
                        account.BankName,
                    AccountNumber =
                        account.AccountNumber,
                    IFSC =
                        account.IFSC,
                    Branch =
                        account.Branch,
                    UpiId =
                        account.UpiId,
                    ExistingQrCodePath =
                        account.QrCodePath,
                    IsActive =
                        account.IsActive
                };
            // -----------------------------------------------------
            // RETURN EDIT PARTIAL
            // -----------------------------------------------------
            return PartialView(
                "_EditPartial",
                model);
        }
        // =========================================================
        // EDIT - POST
        // AJAX POPUP
        //
        // POST: /admin/paymentaccounts/edit/5
        // =========================================================
        [HttpPost]
        [Route("/Admin/PaymentAccounts/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PaymentAccountViewModel model)
        {
            // -----------------------------------------------------
            // VERIFY ID
            // -----------------------------------------------------
            if (id != model.Id)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Invalid payment account."
                });
            }
            // -----------------------------------------------------
            // LOAD EXISTING ACCOUNT
            // -----------------------------------------------------
            var account =
                await _context.PaymentAccounts
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Payment account not found."
                });
            }
            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                model.ExistingQrCodePath =
                    account.QrCodePath;
                return PartialView(
                    "_EditPartial",
                    model);
            }
            // -----------------------------------------------------
            // SAVE OLD QR PATH
            // -----------------------------------------------------
            var oldQrCodePath =
                account.QrCodePath;
            string? newQrCodePath = null;
            try
            {
                // -------------------------------------------------
                // UPDATE ACCOUNT INFORMATION
                // -------------------------------------------------
                account.AccountName =
                    model.AccountName.Trim();
                account.BankName =
                    model.BankName.Trim();
                account.AccountNumber =
                    model.AccountNumber.Trim();
                account.IFSC =
                    model.IFSC
                        .Trim()
                        .ToUpperInvariant();
                account.Branch =
                    string.IsNullOrWhiteSpace(
                        model.Branch)
                        ? null
                        : model.Branch.Trim();
                account.UpiId =
                    string.IsNullOrWhiteSpace(
                        model.UpiId)
                        ? null
                        : model.UpiId.Trim();
                account.IsActive =
                    model.IsActive;
                account.UpdatedAt =
                    DateTime.UtcNow;
                // -------------------------------------------------
                // NEW QR CODE
                // -------------------------------------------------
                if (model.QrCode != null &&
                    model.QrCode.Length > 0)
                {
                    newQrCodePath =
                        await _qrCodeService.SaveAsync(
                            model.QrCode);
                    account.QrCodePath =
                        newQrCodePath;
                }
                // -------------------------------------------------
                // SAVE DATABASE
                // -------------------------------------------------
                await _context.SaveChangesAsync();
                // -------------------------------------------------
                // DELETE OLD QR AFTER SUCCESS
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                        newQrCodePath) &&
                    !string.IsNullOrWhiteSpace(
                        oldQrCodePath) &&
                    !string.Equals(
                        newQrCodePath,
                        oldQrCodePath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await _qrCodeService.DeleteAsync(
                        oldQrCodePath);
                }
                // -------------------------------------------------
                // SUCCESS MESSAGE
                // -------------------------------------------------
                TempData["SuccessMessage"] =
                    "Payment account updated successfully.";
                // -------------------------------------------------
                // AJAX SUCCESS
                // -------------------------------------------------
                return Json(new
                {
                    success = true,
                    message =
                        "Payment account updated successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                // -------------------------------------------------
                // REMOVE NEW QR IF UPDATE FAILED
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    newQrCodePath))
                {
                    await _qrCodeService.DeleteAsync(
                        newQrCodePath);
                }
                model.ExistingQrCodePath =
                    oldQrCodePath;
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
                return PartialView(
                    "_EditPartial",
                    model);
            }
            catch (Exception ex)
            {
                // -------------------------------------------------
                // REMOVE NEW QR IF UPDATE FAILED
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    newQrCodePath))
                {
                    await _qrCodeService.DeleteAsync(
                        newQrCodePath);
                }
                model.ExistingQrCodePath =
                    oldQrCodePath;
                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to update payment account: {ex.Message}");
                return PartialView(
                    "_EditPartial",
                    model);
            }
        }
        // =========================================================
        // TOGGLE ACTIVE / INACTIVE
        //
        // POST: /admin/paymentaccounts/toggleactive/5
        // =========================================================
        [HttpPost]
        [Route("/Admin/PaymentAccounts/ToggleActive/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(
            int id)
        {
            var account =
                await _context.PaymentAccounts
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (account == null)
            {
                TempData["ErrorMessage"] =
                    "Payment account not found.";
                return Redirect(
                    "/Admin/PaymentAccounts");
            }
            // -----------------------------------------------------
            // TOGGLE
            // -----------------------------------------------------
            account.IsActive =
                !account.IsActive;
            account.UpdatedAt =
                DateTime.UtcNow;
            await _context.SaveChangesAsync();
            // -----------------------------------------------------
            // MESSAGE
            // -----------------------------------------------------
            TempData["SuccessMessage"] =
                account.IsActive
                    ? $"'{account.AccountName}' is now active and will be displayed on the public donation page."
                    : $"'{account.AccountName}' has been deactivated and will no longer be displayed on the public donation page.";
            return Redirect(
                "/Admin/PaymentAccounts");
        }
        // =========================================================
        // DELETE - GET
        // AJAX POPUP
        //
        // GET: /admin/paymentaccounts/delete/5
        // =========================================================
        [HttpGet]
        [Route("/Admin/PaymentAccounts/Delete/{id:int}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            var account =
                await _context.PaymentAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Payment account not found."
                });
            }
            // -----------------------------------------------------
            // RETURN DELETE PARTIAL
            // -----------------------------------------------------
            return PartialView(
                "_DeletePartial",
                account);
        }
        // =========================================================
        // DELETE - POST
        // AJAX
        //
        // POST: /admin/paymentaccounts/delete/5
        // =========================================================
        [HttpPost]
        [Route("/Admin/PaymentAccounts/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            // -----------------------------------------------------
            // LOAD ACCOUNT
            // -----------------------------------------------------
            var account =
                await _context.PaymentAccounts
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (account == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Payment account not found."
                });
            }
            // -----------------------------------------------------
            // SAVE QR PATH BEFORE DELETE
            // -----------------------------------------------------
            var qrCodePath =
                account.QrCodePath;
            try
            {
                // -------------------------------------------------
                // DELETE DATABASE RECORD
                // -------------------------------------------------
                _context.PaymentAccounts.Remove(
                    account);
                await _context.SaveChangesAsync();
                // -------------------------------------------------
                // DELETE QR FILE
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    qrCodePath))
                {
                    await _qrCodeService.DeleteAsync(
                        qrCodePath);
                }
                // -------------------------------------------------
                // SUCCESS MESSAGE
                // -------------------------------------------------
                TempData["SuccessMessage"] =
                    "Payment account deleted successfully.";
                // -------------------------------------------------
                // AJAX SUCCESS
                // -------------------------------------------------
                return Json(new
                {
                    success = true,
                    message =
                        "Payment account deleted successfully."
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
                            $"Unable to delete payment account: {ex.Message}"
                    });
            }
        }
    }
}