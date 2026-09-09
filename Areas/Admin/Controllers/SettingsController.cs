using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRole.Administrator)]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SettingsController(
            ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // GET: /Admin/Settings
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var setting = await _context.FoundationSettings
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
            if (setting == null)
            {
                return View(new FoundationSettingViewModel
                {
                    FoundationName = "Foundation",
                    Tagline =
                        "Serving with compassion • Supporting with purpose",
                    ReceiptFooter =
                        "Thank you for your generous contribution.",
                    ThankYouMessage =
                        "Your contribution makes a difference."
                });
            }
            return View(new FoundationSettingViewModel
            {
                FoundationName =
                    setting.FoundationName,
                Tagline =
                    setting.Tagline,
                Address =
                    setting.Address,
                Phone =
                    setting.Phone,
                Email =
                    setting.Email,
                Website =
                    setting.Website,
                RegistrationNumber =
                    setting.RegistrationNumber,
                PAN =
                    setting.PAN,
                LogoPath =
                    setting.LogoPath,
                ReceiptFooter =
                    setting.ReceiptFooter,
                ThankYouMessage =
                    setting.ThankYouMessage
            });
        }
        // =========================================================
        // POST: /Admin/Settings
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            FoundationSettingViewModel model)
        {
            // -----------------------------------------------------
            // Validation
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null &&
                                x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors
                        .Select(error =>
                            string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? $"{x.Key}: Invalid value."
                                : $"{x.Key}: {error.ErrorMessage}"))
                    .ToList();
                TempData["ErrorMessage"] =
                    errors.Count > 0
                        ? string.Join(" | ", errors)
                        : "Please correct the highlighted fields.";
                return View(model);
            }
            try
            {
                var setting = await _context.FoundationSettings
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();
                // -------------------------------------------------
                // CREATE
                // -------------------------------------------------
                if (setting == null)
                {
                    setting = new FoundationSetting
                    {
                        FoundationName =
                            model.FoundationName.Trim(),
                        Tagline =
                            Clean(model.Tagline),
                        Address =
                            Clean(model.Address),
                        Phone =
                            Clean(model.Phone),
                        Email =
                            Clean(model.Email),
                        Website =
                            Clean(model.Website),
                        RegistrationNumber =
                            Clean(model.RegistrationNumber),
                        PAN =
                            Clean(model.PAN),
                        LogoPath =
                            Clean(model.LogoPath),
                        ReceiptFooter =
                            Clean(model.ReceiptFooter),
                        ThankYouMessage =
                            Clean(model.ThankYouMessage),
                        CreatedAt =
                            DateTime.UtcNow
                    };
                    _context.FoundationSettings.Add(setting);
                }
                // -------------------------------------------------
                // UPDATE
                // -------------------------------------------------
                else
                {
                    setting.FoundationName =
                        model.FoundationName.Trim();
                    setting.Tagline =
                        Clean(model.Tagline);
                    setting.Address =
                        Clean(model.Address);
                    setting.Phone =
                        Clean(model.Phone);
                    setting.Email =
                        Clean(model.Email);
                    setting.Website =
                        Clean(model.Website);
                    setting.RegistrationNumber =
                        Clean(model.RegistrationNumber);
                    setting.PAN =
                        Clean(model.PAN);
                    setting.LogoPath =
                        Clean(model.LogoPath);
                    setting.ReceiptFooter =
                        Clean(model.ReceiptFooter);
                    setting.ThankYouMessage =
                        Clean(model.ThankYouMessage);
                    setting.UpdatedAt =
                        DateTime.UtcNow;
                    _context.FoundationSettings.Update(setting);
                }
                // -------------------------------------------------
                // SAVE
                // -------------------------------------------------
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] =
                    "Foundation settings have been saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The settings could not be saved to the database. " +
                    "Please make sure the FoundationSettings database table exists.");
                // Log the actual database error in Development
                if (HttpContext.RequestServices
                    .GetRequiredService<
                        ILogger<SettingsController>>()
                    is ILogger<SettingsController> logger)
                {
                    logger.LogError(
                        ex,
                        "Error while saving Foundation Settings.");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred while saving the settings.");
                if (HttpContext.RequestServices
                    .GetRequiredService<
                        ILogger<SettingsController>>()
                    is ILogger<SettingsController> logger)
                {
                    logger.LogError(
                        ex,
                        "Unexpected error while saving Foundation Settings.");
                }
                return View(model);
            }
        }
        // =========================================================
        // CLEAN STRING
        // =========================================================
        private static string? Clean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return value.Trim();
        }
    }
}