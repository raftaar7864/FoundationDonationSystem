using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRole.Administrator)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // =========================================================
        // INDEX
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            var userList = new List<UserListViewModel>();
            foreach (var user in users)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);
                userList.Add(
                    new UserListViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Designation = user.Designation,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        UserName = user.UserName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        Role = roles.FirstOrDefault()
                    });
            }
            return View(userList);
        }
        // =========================================================
        // CREATE - GET
        // AJAX POPUP
        // GET: /admin/users/create
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            var model =
                new UserCreateViewModel
                {
                    IsActive = true,
                    Role = UserRole.DonationVerifier
                };
            return PartialView(
                "_CreatePartial",
                model);
        }
        // =========================================================
        // CREATE - POST
        // AJAX POPUP
        // POST: /admin/users/create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            UserCreateViewModel model)
        {
            // -----------------------------------------------------
            // MODEL VALIDATION
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            // -----------------------------------------------------
            // ROLE VALIDATION
            // -----------------------------------------------------
            var allowedRoles = new[]
            {
                UserRole.Administrator,
                UserRole.DonationVerifier,
                UserRole.PostManager
            };
            if (!allowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Invalid role selected.");
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Selected role does not exist.");
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            // -----------------------------------------------------
            // NORMALIZE VALUES
            // -----------------------------------------------------
            var username =
                model.UserName.Trim();
            var email =
                string.IsNullOrWhiteSpace(model.Email)
                    ? null
                    : model.Email.Trim();
            // -----------------------------------------------------
            // USERNAME CHECK
            // -----------------------------------------------------
            var existingUsername =
                await _userManager.FindByNameAsync(
                    username);
            if (existingUsername != null)
            {
                ModelState.AddModelError(
                    nameof(model.UserName),
                    "This username is already in use.");
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            // -----------------------------------------------------
            // EMAIL CHECK
            // -----------------------------------------------------
            if (email != null)
            {
                var existingEmail =
                    await _userManager.FindByEmailAsync(
                        email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "This email address is already in use.");
                    return PartialView(
                        "_CreatePartial",
                        model);
                }
            }
            // -----------------------------------------------------
            // CREATE USER
            // -----------------------------------------------------
            var user =
                new ApplicationUser
                {
                    FullName =
                        model.FullName.Trim(),
                    Designation =
                        string.IsNullOrWhiteSpace(
                            model.Designation)
                            ? null
                            : model.Designation.Trim(),
                    UserName =
                        username,
                    Email =
                        email,
                    PhoneNumber =
                        string.IsNullOrWhiteSpace(
                            model.PhoneNumber)
                            ? null
                            : model.PhoneNumber.Trim(),
                    IsActive =
                        model.IsActive,
                    CreatedAt =
                        DateTime.UtcNow
                };
            // -----------------------------------------------------
            // CREATE IDENTITY USER
            // -----------------------------------------------------
            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            // -----------------------------------------------------
            // ASSIGN ROLE
            // -----------------------------------------------------
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    model.Role);
            if (!roleResult.Succeeded)
            {
                // Roll back user creation if role assignment fails.
                await _userManager.DeleteAsync(user);
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------
            return Json(
                new
                {
                    success = true,
                    message = "User created successfully."
                });
        }
        // =========================================================
        // EDIT - GET
        // AJAX POPUP
        // GET: /admin/users/edit/{id}
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }
            var user =
                await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles =
                await _userManager.GetRolesAsync(user);
            var role =
                roles.FirstOrDefault()
                ?? UserRole.DonationVerifier;
            var model =
                new UserEditViewModel
                {
                    Id =
                        user.Id,
                    FullName =
                        user.FullName,
                    Designation =
                        user.Designation,
                    UserName =
                        user.UserName
                        ?? string.Empty,
                    Email =
                        user.Email
                        ?? string.Empty,
                    PhoneNumber =
                        user.PhoneNumber,
                    Role =
                        role,
                    IsActive =
                        user.IsActive,
                    CreatedAt =
                        user.CreatedAt
                };
            return PartialView(
                "_EditPartial",
                model);
        }
        // =========================================================
        // EDIT - POST
        // AJAX POPUP
        // POST: /admin/users/edit/{id}
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            UserEditViewModel model)
        {
            // -----------------------------------------------------
            // MODEL VALIDATION
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_EditPartial",
                    model);
            }
            // -----------------------------------------------------
            // LOAD USER
            // -----------------------------------------------------
            var user =
                await _userManager.FindByIdAsync(
                    model.Id);
            if (user == null)
            {
                return NotFound(
                    new
                    {
                        success = false,
                        message = "User not found."
                    });
            }
            // -----------------------------------------------------
            // ROLE VALIDATION
            // -----------------------------------------------------
            var allowedRoles = new[]
            {
                UserRole.Administrator,
                UserRole.DonationVerifier,
                UserRole.PostManager
            };
            if (!allowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Invalid role selected.");
                return PartialView(
                    "_EditPartial",
                    model);
            }
            if (!await _roleManager.RoleExistsAsync(
                    model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Selected role does not exist.");
                return PartialView(
                    "_EditPartial",
                    model);
            }
            // -----------------------------------------------------
            // NORMALIZE USERNAME
            // -----------------------------------------------------
            var username =
                model.UserName.Trim();
            // -----------------------------------------------------
            // USERNAME CHECK
            // -----------------------------------------------------
            var existingUsername =
                await _userManager.FindByNameAsync(
                    username);
            if (
                existingUsername != null &&
                existingUsername.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.UserName),
                    "This username is already in use.");
                return PartialView(
                    "_EditPartial",
                    model);
            }
            // -----------------------------------------------------
            // NORMALIZE EMAIL
            // -----------------------------------------------------
            var email =
                string.IsNullOrWhiteSpace(model.Email)
                    ? null
                    : model.Email.Trim();
            // -----------------------------------------------------
            // EMAIL CHECK
            // -----------------------------------------------------
            if (email != null)
            {
                var existingEmail =
                    await _userManager.FindByEmailAsync(
                        email);
                if (
                    existingEmail != null &&
                    existingEmail.Id != user.Id)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "This email address is already in use.");
                    return PartialView(
                        "_EditPartial",
                        model);
                }
            }
            // =====================================================
            // CURRENT ROLE
            // =====================================================
            var currentRoles =
                await _userManager.GetRolesAsync(user);
            var currentRole =
                currentRoles.FirstOrDefault();
            var isCurrentlyAdministrator =
                currentRoles.Contains(
                    UserRole.Administrator);
            var changingAdministratorRole =
                !string.Equals(
                    currentRole,
                    model.Role,
                    StringComparison.OrdinalIgnoreCase);
            // =====================================================
            // PROTECT LAST ADMINISTRATOR
            // =====================================================
            if (
                isCurrentlyAdministrator &&
                changingAdministratorRole)
            {
                var administrators =
                    await _userManager.GetUsersInRoleAsync(
                        UserRole.Administrator);
                var activeAdministrators =
                    administrators.Count(
                        x => x.IsActive);
                if (activeAdministrators <= 1)
                {
                    ModelState.AddModelError(
                        nameof(model.Role),
                        "The last active Administrator cannot be removed.");
                    return PartialView(
                        "_EditPartial",
                        model);
                }
            }
            // =====================================================
            // PROTECT LAST ACTIVE ADMINISTRATOR
            // =====================================================
            if (
                isCurrentlyAdministrator &&
                !model.IsActive)
            {
                var administrators =
                    await _userManager.GetUsersInRoleAsync(
                        UserRole.Administrator);
                var activeAdministrators =
                    administrators.Count(
                        x =>
                            x.IsActive &&
                            x.Id != user.Id);
                if (activeAdministrators <= 0)
                {
                    ModelState.AddModelError(
                        nameof(model.IsActive),
                        "The last active Administrator cannot be deactivated.");
                    return PartialView(
                        "_EditPartial",
                        model);
                }
            }
            // =====================================================
            // UPDATE USER DETAILS
            // =====================================================
            user.FullName =
                model.FullName.Trim();
            user.Designation =
                string.IsNullOrWhiteSpace(
                    model.Designation)
                    ? null
                    : model.Designation.Trim();
            user.UserName =
                username;
            user.Email =
                email;
            user.PhoneNumber =
                string.IsNullOrWhiteSpace(
                    model.PhoneNumber)
                    ? null
                    : model.PhoneNumber.Trim();
            user.IsActive =
                model.IsActive;
            // -----------------------------------------------------
            // UPDATE IDENTITY USER
            // -----------------------------------------------------
            var updateResult =
                await _userManager.UpdateAsync(
                    user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }
                return PartialView(
                    "_EditPartial",
                    model);
            }
            // =====================================================
            // UPDATE ROLE
            // =====================================================
            currentRoles =
                await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                var removeResult =
                    await _userManager.RemoveFromRolesAsync(
                        user,
                        currentRoles);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }
                    return PartialView(
                        "_EditPartial",
                        model);
                }
            }
            var addRoleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    model.Role);
            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }
                return PartialView(
                    "_EditPartial",
                    model);
            }
            // =====================================================
            // PASSWORD CHANGE
            // =====================================================
            if (!string.IsNullOrWhiteSpace(
                    model.NewPassword))
            {
                var token =
                    await _userManager
                        .GeneratePasswordResetTokenAsync(
                            user);
                var passwordResult =
                    await _userManager
                        .ResetPasswordAsync(
                            user,
                            token,
                            model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(
                            nameof(model.NewPassword),
                            error.Description);
                    }
                    return PartialView(
                        "_EditPartial",
                        model);
                }
            }
            // =====================================================
            // SUCCESS
            // =====================================================
            return Json(
                new
                {
                    success = true,
                    message = "User updated successfully."
                });
        }
        // =========================================================
        // DETAILS
        // AJAX POPUP
        // GET: /admin/users/details/{id}
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }
            var user =
                await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(
                    new
                    {
                        success = false,
                        message = "User not found."
                    });
            }
            var roles =
                await _userManager.GetRolesAsync(user);
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
                    IsActive =
                        user.IsActive,
                    Role =
                        roles.FirstOrDefault(),
                    CreatedAt =
                        user.CreatedAt
                };
            return PartialView(
                "_DetailsPartial",
                model);
        }
    }
}