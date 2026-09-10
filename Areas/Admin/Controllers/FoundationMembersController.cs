using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class FoundationMembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private const long MaxImageSize = 2 * 1024 * 1024;
        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };
        private static readonly string[] AllowedImageContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };
        private const string ImageFolder =
            "images/foundation/leadership";
        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public FoundationMembersController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        // =========================================================
        // GET: Admin/FoundationMembers
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var members = await _context.FoundationMembers
                .AsNoTracking()
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();
            return View(members);
        }
        // =========================================================
        // GET: Admin/FoundationMembers/Create
        // Returns Create Partial
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            var model = new FoundationMember
            {
                IsActive = true,
                DisplayOrder = 0
            };
            return PartialView(
                "_CreatePartial",
                model
            );
        }
        // =========================================================
        // POST: Admin/FoundationMembers/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            FoundationMember model)
        {
            // -----------------------------------------------------
            // Validate uploaded image
            // -----------------------------------------------------
            if (model.ProfileImageFile != null)
            {
                ValidateImage(
                    model.ProfileImageFile,
                    nameof(model.ProfileImageFile)
                );
            }
            // -----------------------------------------------------
            // Return validation errors
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_CreatePartial",
                    model
                );
            }
            // -----------------------------------------------------
            // Clean input
            // -----------------------------------------------------
            model.FullName =
                model.FullName.Trim();
            model.Designation =
                model.Designation.Trim();
            model.ShortBio =
                string.IsNullOrWhiteSpace(model.ShortBio)
                    ? null
                    : model.ShortBio.Trim();
            // -----------------------------------------------------
            // Upload profile image
            // -----------------------------------------------------
            if (model.ProfileImageFile != null &&
                model.ProfileImageFile.Length > 0)
            {
                model.ProfileImage =
                    await SaveImageAsync(
                        model.ProfileImageFile
                    );
            }
            else
            {
                model.ProfileImage = null;
            }
            // -----------------------------------------------------
            // System values
            // -----------------------------------------------------
            model.CreatedAt =
                DateTime.UtcNow;
            model.UpdatedAt =
                null;
            // -----------------------------------------------------
            // Save member
            // -----------------------------------------------------
            _context.FoundationMembers.Add(model);
            await _context.SaveChangesAsync();
            // -----------------------------------------------------
            // Success message
            // -----------------------------------------------------
            TempData["SuccessMessage"] =
                "Foundation member added successfully.";
            return RedirectToAction(
                nameof(Index)
            );
        }
        // =========================================================
        // GET: Admin/FoundationMembers/Edit/5
        // Returns Edit Partial
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var member =
                await _context.FoundationMembers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id.Value
                    );
            if (member == null)
            {
                return NotFound();
            }
            return PartialView(
                "_EditPartial",
                member
            );
        }
        // =========================================================
        // POST: Admin/FoundationMembers/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            FoundationMember model)
        {
            // -----------------------------------------------------
            // Validate ID
            // -----------------------------------------------------
            if (id != model.Id)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // Validate new image
            // -----------------------------------------------------
            if (model.ProfileImageFile != null)
            {
                ValidateImage(
                    model.ProfileImageFile,
                    nameof(model.ProfileImageFile)
                );
            }
            // -----------------------------------------------------
            // Return validation errors
            // -----------------------------------------------------
            if (!ModelState.IsValid)
            {
                return PartialView(
                    "_EditPartial",
                    model
                );
            }
            // -----------------------------------------------------
            // Find existing member
            // -----------------------------------------------------
            var member =
                await _context.FoundationMembers
                    .FirstOrDefaultAsync(
                        x => x.Id == id
                    );
            if (member == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // Store old image path BEFORE making changes
            // -----------------------------------------------------
            var oldImage =
                member.ProfileImage;
            // -----------------------------------------------------
            // Clean input
            // -----------------------------------------------------
            member.FullName =
                model.FullName.Trim();
            member.Designation =
                model.Designation.Trim();
            member.ShortBio =
                string.IsNullOrWhiteSpace(model.ShortBio)
                    ? null
                    : model.ShortBio.Trim();
            member.DisplayOrder =
                model.DisplayOrder;
            member.IsActive =
                model.IsActive;
            // =====================================================
            // IMAGE HANDLING
            // =====================================================
            // -----------------------------------------------------
            // Case 1:
            // New image uploaded
            // -----------------------------------------------------
            if (model.ProfileImageFile != null &&
                model.ProfileImageFile.Length > 0)
            {
                var newImage =
                    await SaveImageAsync(
                        model.ProfileImageFile
                    );
                member.ProfileImage =
                    newImage;
                // -------------------------------------------------
                // Delete old image only after new image
                // has been successfully saved
                // -------------------------------------------------
                DeleteImage(oldImage);
            }
            // -----------------------------------------------------
            // Case 2:
            // Remove current image
            // -----------------------------------------------------
            else if (model.RemoveProfileImage)
            {
                member.ProfileImage =
                    null;
                DeleteImage(oldImage);
            }
            // -----------------------------------------------------
            // Case 3:
            // No image change
            // -----------------------------------------------------
            // Existing image remains unchanged.
            // -----------------------------------------------------
            // Updated time
            // -----------------------------------------------------
            member.UpdatedAt =
                DateTime.UtcNow;
            // -----------------------------------------------------
            // Save changes
            // -----------------------------------------------------
            await _context.SaveChangesAsync();
            // -----------------------------------------------------
            // Success message
            // -----------------------------------------------------
            TempData["SuccessMessage"] =
                "Foundation member updated successfully.";
            return RedirectToAction(
                nameof(Index)
            );
        }
        // =========================================================
        // GET: Admin/FoundationMembers/Details/5
        // Returns Details Partial
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var member =
                await _context.FoundationMembers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id.Value
                    );
            if (member == null)
            {
                return NotFound();
            }
            return PartialView(
                "_DetailsPartial",
                member
            );
        }
        // =========================================================
        // GET: Admin/FoundationMembers/Delete/5
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Delete(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var member =
                await _context.FoundationMembers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == id.Value
                    );
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }
        // =========================================================
        // POST: Admin/FoundationMembers/Delete/5
        // =========================================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var member =
                await _context.FoundationMembers
                    .FirstOrDefaultAsync(
                        x => x.Id == id
                    );
            if (member == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // Store image path before deleting database record
            // -----------------------------------------------------
            var profileImage =
                member.ProfileImage;
            // -----------------------------------------------------
            // Delete database record
            // -----------------------------------------------------
            _context.FoundationMembers.Remove(
                member
            );
            await _context.SaveChangesAsync();
            // -----------------------------------------------------
            // Delete profile image from wwwroot
            // -----------------------------------------------------
            DeleteImage(
                profileImage
            );
            // -----------------------------------------------------
            // Success message
            // -----------------------------------------------------
            TempData["SuccessMessage"] =
                "Foundation member deleted successfully.";
            return RedirectToAction(
                nameof(Index)
            );
        }
        // =========================================================
        // POST: Admin/FoundationMembers/ToggleStatus/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(
            int id)
        {
            var member =
                await _context.FoundationMembers
                    .FirstOrDefaultAsync(
                        x => x.Id == id
                    );
            if (member == null)
            {
                return NotFound();
            }
            // -----------------------------------------------------
            // Toggle status
            // -----------------------------------------------------
            member.IsActive =
                !member.IsActive;
            member.UpdatedAt =
                DateTime.UtcNow;
            await _context.SaveChangesAsync();
            // -----------------------------------------------------
            // Success message
            // -----------------------------------------------------
            TempData["SuccessMessage"] =
                member.IsActive
                    ? "Foundation member activated successfully."
                    : "Foundation member deactivated successfully.";
            return RedirectToAction(
                nameof(Index)
            );
        }
        // =========================================================
        // IMAGE VALIDATION
        // =========================================================
        private void ValidateImage(
            IFormFile image,
            string propertyName)
        {
            if (image == null ||
                image.Length == 0)
            {
                return;
            }
            // -----------------------------------------------------
            // File size
            // -----------------------------------------------------
            if (image.Length > MaxImageSize)
            {
                ModelState.AddModelError(
                    propertyName,
                    "Image size must not exceed 2 MB."
                );
                return;
            }
            // -----------------------------------------------------
            // File extension
            // -----------------------------------------------------
            var extension =
                Path.GetExtension(
                    image.FileName
                ).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(
                    extension))
            {
                ModelState.AddModelError(
                    propertyName,
                    "Only JPG, JPEG, PNG and WEBP images are allowed."
                );
            }
            // -----------------------------------------------------
            // Content type
            // -----------------------------------------------------
            var contentType =
                image.ContentType?
                    .ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(contentType) ||
                !AllowedImageContentTypes.Contains(
                    contentType))
            {
                ModelState.AddModelError(
                    propertyName,
                    "Invalid image file type."
                );
            }
        }
        // =========================================================
        // SAVE IMAGE
        // =========================================================
        private async Task<string> SaveImageAsync(
            IFormFile image)
        {
            // -----------------------------------------------------
            // Get wwwroot
            // -----------------------------------------------------
            var webRootPath =
                _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(
                    webRootPath))
            {
                webRootPath =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot"
                    );
            }
            // -----------------------------------------------------
            // Create upload folder
            // -----------------------------------------------------
            var uploadFolder =
                Path.Combine(
                    webRootPath,
                    "images",
                    "foundation",
                    "leadership"
                );
            if (!Directory.Exists(
                    uploadFolder))
            {
                Directory.CreateDirectory(
                    uploadFolder
                );
            }
            // -----------------------------------------------------
            // Get extension
            // -----------------------------------------------------
            var extension =
                Path.GetExtension(
                    image.FileName
                ).ToLowerInvariant();
            // -----------------------------------------------------
            // Generate unique filename
            // -----------------------------------------------------
            var fileName =
                $"{Guid.NewGuid():N}{extension}";
            var filePath =
                Path.Combine(
                    uploadFolder,
                    fileName
                );
            // -----------------------------------------------------
            // Save image
            // -----------------------------------------------------
            await using (
                var stream =
                    new FileStream(
                        filePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None))
            {
                await image.CopyToAsync(
                    stream
                );
            }
            // -----------------------------------------------------
            // Return relative URL
            // -----------------------------------------------------
            return $"/{ImageFolder}/{fileName}";
        }
        // =========================================================
        // DELETE IMAGE
        // =========================================================
        private void DeleteImage(
            string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(
                    imagePath))
            {
                return;
            }
            // -----------------------------------------------------
            // Normalize path
            // -----------------------------------------------------
            var normalizedPath =
                imagePath
                    .Replace(
                        '\\',
                        '/'
                    )
                    .TrimStart('/');
            // -----------------------------------------------------
            // Security check:
            // Only delete files inside our
            // Foundation Leadership folder.
            // -----------------------------------------------------
            var expectedFolder =
                ImageFolder
                    .TrimStart('/')
                    .TrimEnd('/')
                    + "/";
            if (!normalizedPath.StartsWith(
                    expectedFolder,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            // -----------------------------------------------------
            // Get filename only
            // -----------------------------------------------------
            var fileName =
                Path.GetFileName(
                    normalizedPath
                );
            if (string.IsNullOrWhiteSpace(
                    fileName))
            {
                return;
            }
            // -----------------------------------------------------
            // Get wwwroot
            // -----------------------------------------------------
            var webRootPath =
                _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(
                    webRootPath))
            {
                webRootPath =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot"
                    );
            }
            // -----------------------------------------------------
            // Build physical file path
            // -----------------------------------------------------
            var filePath =
                Path.Combine(
                    webRootPath,
                    ImageFolder.Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    ),
                    fileName
                );
            // -----------------------------------------------------
            // Delete file safely
            // -----------------------------------------------------
            try
            {
                if (System.IO.File.Exists(
                        filePath))
                {
                    System.IO.File.Delete(
                        filePath
                    );
                }
            }
            catch
            {
                // Do not fail the database operation
                // if the old image cannot be deleted.
            }
        }
    }
}