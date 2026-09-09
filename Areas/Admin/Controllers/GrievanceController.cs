using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRole.Administrator + "," + UserRole.PostManager + "," + UserRole.DonationVerifier)]
    public class GrievanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public GrievanceController(ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // INDEX
        // SEARCH + STATUS FILTER + PAGINATION
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            int page = 1,
            int pageSize = 10)
        {
            search = search?.Trim();
            status = status?.Trim();
            // -----------------------------------------------------
            // VALIDATE PAGE SIZE
            // -----------------------------------------------------
            var allowedPageSizes = new[] { 10, 20, 50 };
            if (!allowedPageSizes.Contains(pageSize))
            {
                pageSize = 10;
            }
            if (page < 1)
            {
                page = 1;
            }
            // -----------------------------------------------------
            // BASE QUERY
            // -----------------------------------------------------
            var query = _context.Grievances
                .AsNoTracking()
                .AsQueryable();
            // -----------------------------------------------------
            // SEARCH
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.GrievanceNumber.Contains(search) ||
                    x.FullName.Contains(search) ||
                    x.Mobile.Contains(search) ||
                    x.Subject.Contains(search) ||
                    (x.ReferenceNumber != null &&
                     x.ReferenceNumber.Contains(search)));
            }
            // -----------------------------------------------------
            // STATUS FILTER
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(status) &&
                !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status == status);
            }
            // -----------------------------------------------------
            // TOTAL COUNT
            // -----------------------------------------------------
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);
            // -----------------------------------------------------
            // PREVENT INVALID PAGE
            // -----------------------------------------------------
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }
            if (totalPages == 0)
            {
                page = 1;
            }
            // -----------------------------------------------------
            // LOAD PAGINATED DATA
            // -----------------------------------------------------
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // -----------------------------------------------------
            // VIEW DATA
            // -----------------------------------------------------
            ViewBag.Search = search;
            ViewBag.Status = string.IsNullOrWhiteSpace(status)
                ? "All"
                : status;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.StartItem = totalItems == 0
                ? 0
                : ((page - 1) * pageSize) + 1;
            ViewBag.EndItem = totalItems == 0
                ? 0
                : Math.Min(page * pageSize, totalItems);
            return View(
                "~/Areas/Admin/Views/Grievance/Index.cshtml",
                items);
        }
        // =========================================================
        // DETAILS
        // RETURNS PARTIAL FOR AJAX MODAL
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Grievances
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // AJAX REQUEST -> PARTIAL VIEW
            // -----------------------------------------------------
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(
                    "~/Areas/Admin/Views/Grievance/_DetailsPartial.cshtml",
                    item);
            }
            // -----------------------------------------------------
            // NORMAL REQUEST -> FULL DETAILS PAGE
            // -----------------------------------------------------
            return View(
                "~/Areas/Admin/Views/Grievance/Index.cshtml",
                item);
        }
        // =========================================================
        // UPDATE STATUS
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status,
            string? adminRemarks)
        {
            var item = await _context.Grievances
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // NORMALIZE STATUS
            // -----------------------------------------------------
            status = status?.Trim() ?? string.Empty;
            var allowedStatuses = new[]
            {
                "Pending",
                "In Review",
                "Resolved",
                "Rejected"
            };
            if (!allowedStatuses.Contains(status))
            {
                TempData["GrievanceErrorMessage"] =
                    "Invalid grievance status.";
                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            // -----------------------------------------------------
            // UPDATE
            // -----------------------------------------------------
            item.Status = status;
            item.AdminRemarks =
                string.IsNullOrWhiteSpace(adminRemarks)
                    ? null
                    : adminRemarks.Trim();
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["GrievanceSuccessMessage"] =
                "Grievance updated successfully.";
            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}
