using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Controllers
{
    [AllowAnonymous]
    public class GrievanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public GrievanceController(ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // GET: /Grievance
        // =========================================================
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Submit a Grievance";
            return View(
                "~/Views/Grievance/Index.cshtml",
                new GrievanceViewModel()
            );
        }
        // =========================================================
        // POST: /Grievance
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(GrievanceViewModel model)
        {
            // -----------------------------------------------------
            // Validation
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Grievance/Index.cshtml",
                    model
                );
            }
            // -----------------------------------------------------
            // Generate grievance number
            // -----------------------------------------------------
            var number = await GenerateGrievanceNumberAsync();
            // -----------------------------------------------------
            // Create grievance
            // -----------------------------------------------------
            var grievance = new Grievance
            {
                GrievanceNumber = number,
                FullName = model.FullName.Trim(),
                Mobile = model.Mobile.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email)
                    ? null
                    : model.Email.Trim(),
                Category = model.Category.Trim(),
                Subject = model.Subject.Trim(),
                Description = model.Description.Trim(),
                ReferenceNumber = string.IsNullOrWhiteSpace(
                    model.ReferenceNumber)
                    ? null
                    : model.ReferenceNumber.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            // -----------------------------------------------------
            // Save grievance
            // -----------------------------------------------------
            _context.Grievances.Add(grievance);
            await _context.SaveChangesAsync();
            // -----------------------------------------------------
            // SUCCESS
            //
            // Instead of redirecting to Success.cshtml,
            // return the _Success.cshtml partial.
            // -----------------------------------------------------
            ViewData["Title"] = "Grievance Submitted";
            ViewBag.GrievanceNumber = grievance.GrievanceNumber;
            return PartialView(
                "~/Views/Grievance/_SuccessPartial.cshtml"
            );
        }
        // =========================================================
        // GET: /Grievance/Success
        //
        // Optional endpoint if you still want a direct success URL.
        // It also returns the partial.
        // =========================================================
        [HttpGet]
        public IActionResult Success(string number)
        {
            ViewData["Title"] = "Grievance Submitted";
            ViewBag.GrievanceNumber = number;
            return PartialView(
                "~/Views/Grievance/_SuccessPartial.cshtml"
            );
        }
        // =========================================================
        // GENERATE GRIEVANCE NUMBER
        // =========================================================
        private async Task<string> GenerateGrievanceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"GRV-{year}-";
            var last = await _context.Grievances
                .AsNoTracking()
                .Where(x =>
                    x.GrievanceNumber.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.GrievanceNumber)
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