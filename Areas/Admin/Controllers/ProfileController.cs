using FoundationDonationSystem.Models;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        // =========================================================
        // PROFILE
        // GET: /Admin/Profile/Index
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(
                    User);
            if (user == null)
            {
                return Unauthorized();
            }
            var roles =
                await _userManager.GetRolesAsync(
                    user);
            var model =
                new UserDetailsViewModel
                {
                    Id =
                        user.Id,
                    FullName =
                        user.FullName,
                    Designation =
                        user.Designation,
                    Email =
                        user.Email,
                    PhoneNumber =
                        user.PhoneNumber,
                    UserName =
                        user.UserName,
                    Role =
                        roles.FirstOrDefault(),
                    IsActive =
                        user.IsActive,
                    CreatedAt =
                        user.CreatedAt
                };
            return PartialView(
                "_ProfilePartial",
                model);
        }
    }
}