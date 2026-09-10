using FoundationDonationSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Controllers
{
    public class FoundationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FoundationController(
            ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // GET: /Foundation/Leadership
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Leadership()
        {
            var members = await _context.FoundationMembers
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();
            return View(members);
        }
    }
}