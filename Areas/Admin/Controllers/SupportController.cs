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
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SupportController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            int page = 1,
            int pageSize = 10)
        {
            search = search?.Trim();
            status = status?.Trim();
            var allowedPageSizes = new[] { 10, 20, 50 };
            if (!allowedPageSizes.Contains(pageSize))
                pageSize = 10;
            if (page < 1)
                page = 1;
            var query = _context.SupportRequests
                .AsNoTracking()
                .AsQueryable();
            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.RequestNumber.Contains(search) ||
                    x.FullName.Contains(search) ||
                    x.Mobile.Contains(search) ||
                    x.Subject.Contains(search));
            }
            // Status filter
            if (!string.IsNullOrWhiteSpace(status) &&
                !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status == status);
            }
            // Total filtered records
            var totalItems = await query.CountAsync();
            // Pagination
            var totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);
            if (totalPages > 0 && page > totalPages)
                page = totalPages;
            if (totalPages == 0)
                page = 1;
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // =========================================================
            // SUMMARY COUNTS
            // These are based on the same search-filtered dataset.
            // =========================================================
            var pendingCount = await query.CountAsync(
                x => x.Status == "Pending");
            var reviewCount = await query.CountAsync(
                x => x.Status == "In Review");
            var resolvedCount = await query.CountAsync(
                x => x.Status == "Resolved");
            // =========================================================
            // VIEW BAG
            // =========================================================
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
            ViewBag.PendingCount = pendingCount;
            ViewBag.ReviewCount = reviewCount;
            ViewBag.ResolvedCount = resolvedCount;
            ViewBag.PageSizes = allowedPageSizes;
            return View(
                "~/Areas/Admin/Views/Support/Index.cshtml",
                items);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.SupportRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return NotFound();
            // Return partial view when requested through AJAX.
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(
                    "~/Areas/Admin/Views/Support/_DetailsPartial.cshtml",
                    item);
            }
            return View(
                "~/Areas/Admin/Views/Support/Index.cshtml",
                item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status,
            string? adminRemarks)
        {
            var item = await _context.SupportRequests
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return NotFound();
            status = status?.Trim() ?? string.Empty;
            // =========================================================
            // ALLOWED SUPPORT STATUSES
            // =========================================================
            var allowedStatuses = new[]
            {
                "Pending",
                "In Review",
                "Resolved",
                "Rejected"
            };
            if (!allowedStatuses.Contains(status))
            {
                TempData["SupportErrorMessage"] =
                    "Invalid support request status.";
                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            // =========================================================
            // UPDATE
            // =========================================================
            item.Status = status;
            item.AdminRemarks = string.IsNullOrWhiteSpace(adminRemarks)
                ? null
                : adminRemarks.Trim();
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            // Use support-specific TempData keys so the message
            // cannot accidentally appear on Posts, Grievances, etc.
            TempData["SupportSuccessMessage"] =
                "Support request updated successfully.";
            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}
