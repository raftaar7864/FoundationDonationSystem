using FoundationDonationSystem.Data;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Controllers
{
    [AllowAnonymous]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // =========================================================
        // PUBLIC POSTS INDEX
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            int? subCategoryId)
        {
            // =====================================================
            // BASE QUERY
            // =====================================================
            var query = _context.Posts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Where(x => x.IsPublished)
                .AsQueryable();
            // =====================================================
            // SEARCH FILTER
            // Searches:
            // - Title
            // - Short Description
            // - Content
            // - Category Name
            // - Sub Category Name
            // =====================================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    (x.ShortDescription != null &&
                     x.ShortDescription.Contains(search)) ||
                    (x.Content != null &&
                     x.Content.Contains(search)) ||
                    (x.Category != null &&
                     x.Category.Name.Contains(search)) ||
                    (x.SubCategory != null &&
                     x.SubCategory.Name.Contains(search))
                );
            }
            // =====================================================
            // CATEGORY FILTER
            // =====================================================
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(x =>
                    x.CategoryId == categoryId.Value);
            }
            // =====================================================
            // SUB CATEGORY FILTER
            // =====================================================
            if (subCategoryId.HasValue && subCategoryId.Value > 0)
            {
                query = query.Where(x =>
                    x.SubCategoryId == subCategoryId.Value);
            }
            // =====================================================
            // POSTS
            // =====================================================
            var posts = await query
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.PostDate)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x =>
                    new PublicPostListViewModel
                    {
                        Id =
                            x.Id,
                        Title =
                            x.Title,
                        ShortDescription =
                            x.ShortDescription,
                        FeaturedImagePath =
                            x.FeaturedImagePath,
                        PostDate =
                            x.PostDate,
                        // Category
                        CategoryId =
                            x.CategoryId,
                        CategoryName =
                            x.Category != null
                                ? x.Category.Name
                                : null,
                        // Sub Category
                        SubCategoryId =
                            x.SubCategoryId,
                        SubCategoryName =
                            x.SubCategory != null
                                ? x.SubCategory.Name
                                : null
                    })
                .ToListAsync();
            // =====================================================
            // ACTIVE CATEGORIES
            // =====================================================
            ViewBag.PostCategories =
                await _context.PostCategories
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .ToListAsync();
            // =====================================================
            // ALL ACTIVE SUB CATEGORIES
            //
            // We load all active subcategories because the
            // public page JavaScript dynamically filters the
            // subcategory dropdown according to the selected
            // category.
            // =====================================================
            ViewBag.PostSubCategories =
                await _context.PostSubCategories
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.PostCategoryId)
                    .ThenBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .ToListAsync();
            // =====================================================
            // CURRENT FILTER VALUES
            // =====================================================
            ViewBag.Search =
                search;
            ViewBag.CategoryId =
                categoryId;
            ViewBag.SubCategoryId =
                subCategoryId;
            // =====================================================
            // PAGE META
            // =====================================================
            ViewData["Title"] =
                "Latest Updates";
            ViewData["Description"] =
                "Read the latest updates, activities and work from our foundation.";
            // =====================================================
            // RETURN VIEW
            // =====================================================
            return View(
                "~/Views/Posts/Index.cshtml",
                posts);
        }
        // =========================================================
        // PUBLIC POST DETAILS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // =====================================================
            // GET PUBLISHED POST
            // =====================================================
            var post =
                await _context.Posts
                    .AsNoTracking()
                    .Include(x => x.Images)
                    .Where(x =>
                        x.Id == id &&
                        x.IsPublished)
                    .FirstOrDefaultAsync();
            // =====================================================
            // POST NOT FOUND
            // =====================================================
            if (post == null)
            {
                return NotFound();
            }
            // =====================================================
            // GALLERY IMAGES
            // =====================================================
            var gallery =
                post.Images
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Id)
                    .Take(3)
                    .ToList();
            var galleryImage1 =
                gallery
                    .FirstOrDefault(
                        x => x.DisplayOrder == 1)
                    ?.ImagePath;
            var galleryImage2 =
                gallery
                    .FirstOrDefault(
                        x => x.DisplayOrder == 2)
                    ?.ImagePath;
            var galleryImage3 =
                gallery
                    .FirstOrDefault(
                        x => x.DisplayOrder == 3)
                    ?.ImagePath;
            // =====================================================
            // VIEW MODEL
            // =====================================================
            var model =
                new PublicPostDetailsViewModel
                {
                    Id =
                        post.Id,
                    Title =
                        post.Title,
                    ShortDescription =
                        post.ShortDescription,
                    Content =
                        post.Content,
                    FeaturedImagePath =
                        post.FeaturedImagePath,
                    PostDate =
                        post.PostDate,
                    GalleryImage1Path =
                        galleryImage1,
                    GalleryImage2Path =
                        galleryImage2,
                    GalleryImage3Path =
                        galleryImage3
                };
            // =====================================================
            // PAGE META
            // =====================================================
            ViewData["Title"] =
                post.Title;
            ViewData["Description"] =
                string.IsNullOrWhiteSpace(
                    post.ShortDescription)
                    ? "Foundation update"
                    : post.ShortDescription;
            // =====================================================
            // RETURN DETAILS VIEW
            // =====================================================
            return View(
                "~/Views/Posts/Details.cshtml",
                model);
        }
    }
}