using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Controllers
{
    [AllowAnonymous]
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SupportController(ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // GET: /Support
        // =========================================================
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Request Support";
            return View(
                "~/Views/Support/Index.cshtml",
                new SupportRequestViewModel()
            );
        }
        // =========================================================
        // POST: /Support
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            SupportRequestViewModel model)
        {
            // -----------------------------------------------------
            // Validation
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Support/Index.cshtml",
                    model
                );
            }
            // -----------------------------------------------------
            // Generate support request number
            // -----------------------------------------------------
            var number = await GenerateRequestNumberAsync();
            // -----------------------------------------------------
            // Create support request
            // -----------------------------------------------------
            var request = new SupportRequest
            {
                RequestNumber = number,
                FullName = model.FullName.Trim(),
                Mobile = model.Mobile.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email)
                    ? null
                    : model.Email.Trim(),
                Address = string.IsNullOrWhiteSpace(model.Address)
                    ? null
                    : model.Address.Trim(),
                SupportCategory = model.SupportCategory.Trim(),
                Subject = model.Subject.Trim(),
                Description = model.Description.Trim(),
                PreferredContactMethod =
                    model.PreferredContactMethod,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            // -----------------------------------------------------
            // Save support request
            // -----------------------------------------------------
            _context.SupportRequests.Add(request);
            await _context.SaveChangesAsync();
            // -----------------------------------------------------
            // SUCCESS
            //
            // Return _Success.cshtml partial instead of
            // redirecting to Success.cshtml.
            // -----------------------------------------------------
            ViewData["Title"] = "Support Request Submitted";
            ViewBag.RequestNumber = request.RequestNumber;
            return PartialView(
                "~/Views/Support/_SuccessPartial.cshtml"
            );
        }
        // =========================================================
        // GET: /Support/Success
        //
        // Optional direct success endpoint.
        // It also returns the success partial.
        // =========================================================
        [HttpGet]
        public IActionResult Success(string number)
        {
            ViewData["Title"] = "Support Request Submitted";
            ViewBag.RequestNumber = number;
            return PartialView(
                "~/Views/Support/_SuccessPartial.cshtml"
            );
        }
        // =========================================================
        // GENERATE SUPPORT REQUEST NUMBER
        // =========================================================
        private async Task<string> GenerateRequestNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"SUP-{year}-";
            var last = await _context.SupportRequests
                .AsNoTracking()
                .Where(x =>
                    x.RequestNumber.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.RequestNumber)
                .FirstOrDefaultAsync();
            var next = 1;
            if (!string.IsNullOrWhiteSpace(last))
            {
                var tail = last[prefix.Length..];
                if (int.TryParse(tail, out var value))
                {
                    next = value + 1;
                }
            }
            return $"{prefix}{next:00000}";
        }
    }
}