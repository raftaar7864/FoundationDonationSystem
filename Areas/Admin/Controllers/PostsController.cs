using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRole.Administrator + "," + UserRole.PostManager)]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        public PostsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }
        // =========================================================
        // INDEX
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? sort,
            int? categoryId,
            int? subCategoryId,
            int page = 1,
            int pageSize = 9)
        {
            search = search?.Trim();
            status = status?.Trim().ToLowerInvariant();
            sort = string.IsNullOrWhiteSpace(sort)
                ? "newest"
                : sort.Trim().ToLowerInvariant();
            if (status != "published" &&
                status != "draft")
            {
                status = null;
            }
            if (sort != "newest" &&
                sort != "oldest" &&
                sort != "date" &&
                sort != "order")
            {
                sort = "newest";
            }
            pageSize = pageSize switch
            {
                18 => 18,
                45 => 45,
                _ => 9
            };
            page = Math.Max(1, page);
            var baseQuery = _context.Posts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.SubCategory);
            // =====================================================
            // STATS
            // =====================================================
            var totalPosts =
                await baseQuery.CountAsync();
            var publishedPosts =
                await baseQuery.CountAsync(
                    x => x.IsPublished);
            var draftPosts =
                totalPosts - publishedPosts;
            var postsWithImage =
                await baseQuery.CountAsync(
                    x => !string.IsNullOrWhiteSpace(
                        x.FeaturedImagePath));
            // =====================================================
            // FILTER
            // =====================================================
            var query =
                baseQuery.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    (x.ShortDescription != null &&
                     x.ShortDescription.Contains(search)) ||
                    (x.CreatedBy != null &&
                     x.CreatedBy.Contains(search)) ||
                    (x.Category != null &&
                     x.Category.Name.Contains(search)) ||
                    (x.SubCategory != null &&
                     x.SubCategory.Name.Contains(search)));
            }
            if (status == "published")
            {
                query = query.Where(
                    x => x.IsPublished);
            }
            else if (status == "draft")
            {
                query = query.Where(
                    x => !x.IsPublished);
            }
            // =====================================================
            // CATEGORY FILTER
            // =====================================================
            if (categoryId.HasValue &&
                categoryId.Value > 0)
            {
                query = query.Where(
                    x => x.CategoryId == categoryId.Value);
            }
            // =====================================================
            // SUBCATEGORY FILTER
            // =====================================================
            if (subCategoryId.HasValue &&
                subCategoryId.Value > 0)
            {
                query = query.Where(
                    x => x.SubCategoryId == subCategoryId.Value);
            }
            // =====================================================
            // SORT
            // =====================================================
            query = sort switch
            {
                "oldest" =>
                    query.OrderBy(x => x.CreatedAt)
                         .ThenBy(x => x.Id),
                "date" =>
                    query.OrderByDescending(
                             x => x.PostDate)
                         .ThenBy(x => x.DisplayOrder)
                         .ThenByDescending(
                             x => x.CreatedAt),
                "order" =>
                    query.OrderBy(
                             x => x.DisplayOrder)
                         .ThenByDescending(
                             x => x.PostDate)
                         .ThenByDescending(
                             x => x.CreatedAt),
                _ =>
                    query.OrderByDescending(
                             x => x.CreatedAt)
                         .ThenByDescending(
                             x => x.Id)
            };
            // =====================================================
            // PAGINATION
            // =====================================================
            var filteredPosts =
                await query.CountAsync();
            var totalPages =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        filteredPosts /
                        (double)pageSize));
            if (page > totalPages)
            {
                page = totalPages;
            }
            var posts =
                await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x =>
                        new PostListViewModel
                        {
                            Id = x.Id,
                            Title = x.Title,
                            ShortDescription =
                                x.ShortDescription,
                            FeaturedImagePath =
                                x.FeaturedImagePath,
                            PostDate =
                                x.PostDate,
                            DisplayOrder =
                                x.DisplayOrder,
                            IsPublished =
                                x.IsPublished,
                            CreatedBy =
                                x.CreatedBy,
                            CreatedAt =
                                x.CreatedAt,
                            UpdatedAt =
                                x.UpdatedAt,
                            CategoryName =
                                x.Category != null
                                    ? x.Category.Name
                                    : null,
                            SubCategoryName =
                                x.SubCategory != null
                                    ? x.SubCategory.Name
                                    : null
                        })
                    .ToListAsync();
            ViewBag.TotalPosts =
                totalPosts;
            ViewBag.PublishedPosts =
                publishedPosts;
            ViewBag.DraftPosts =
                draftPosts;
            ViewBag.PostsWithImage =
                postsWithImage;
            ViewBag.Search =
                search ?? string.Empty;
            ViewBag.Status =
                status ?? string.Empty;
            ViewBag.Sort =
                sort;
            ViewBag.CategoryId =
                categoryId;
            ViewBag.SubCategoryId =
                subCategoryId;
            ViewBag.PostCategories =
                await GetActiveCategoriesAsync();
            // Load ALL active subcategories for the Index filter.
            // JavaScript filters them by data-category-id.
            ViewBag.PostSubCategories =
                await GetAllActiveSubCategoriesAsync();
            ViewBag.PageSize =
                pageSize;
            ViewBag.CurrentPage =
                page;
            ViewBag.TotalPages =
                totalPages;
            ViewBag.FilteredPosts =
                filteredPosts;
            ViewBag.StartItem =
                filteredPosts == 0
                    ? 0
                    : ((page - 1) * pageSize) + 1;
            ViewBag.EndItem =
                Math.Min(
                    page * pageSize,
                    filteredPosts);
            return View(posts);
        }
        // =========================================================
        // CREATE - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PostCreateViewModel
            {
                PostDate = DateTime.Today,
                DisplayOrder = 0,
                IsPublished = false,
                // Default category
                CategoryId = 1,
                SubCategoryId = null
            };
            await LoadCategoryDataAsync(
                model.CategoryId,
                model.SubCategoryId);
            return PartialView("_CreatePartial", model);
        }
        private async Task<bool> ValidateCategorySelectionAsync(
            int categoryId,
            int? subCategoryId)
        {
            var category = await _context.PostCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == categoryId &&
                    x.IsActive);
            if (category == null)
            {
                ModelState.AddModelError(
                    nameof(PostCreateViewModel.CategoryId),
                    "Please select a valid category.");
                return false;
            }
            // Activities requires a valid subcategory.
            if (string.Equals(
                    category.Name,
                    "Activities",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (!subCategoryId.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(PostCreateViewModel.SubCategoryId),
                        "Please select a subcategory.");
                    return false;
                }
                var validSubCategory =
                    await _context.PostSubCategories
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.Id == subCategoryId.Value &&
                            x.PostCategoryId == categoryId &&
                            x.IsActive);
                if (!validSubCategory)
                {
                    ModelState.AddModelError(
                        nameof(PostCreateViewModel.SubCategoryId),
                        "Please select a valid subcategory.");
                    return false;
                }
            }
            else
            {
                // Announcement and News do not use subcategories.
                if (subCategoryId.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(PostCreateViewModel.SubCategoryId),
                        "Subcategory is not applicable for this category.");
                    return false;
                }
            }
            return true;
        }
        // =========================================================
        // CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PostCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoryDataAsync(
                    model.CategoryId,
                    model.SubCategoryId);
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            if (!await ValidateCategorySelectionAsync(
                    model.CategoryId,
                    model.SubCategoryId))
            {
                await LoadCategoryDataAsync(
                    model.CategoryId,
                    model.SubCategoryId);
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            var user =
                await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Unable to identify the current user."
                });
            }
            // =====================================================
            // FEATURED IMAGE
            // =====================================================
            string? featuredImagePath = null;
            if (model.FeaturedImage != null)
            {
                var uploadResult =
                    await SaveFeaturedImageAsync(
                        model.FeaturedImage);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError(
                        nameof(model.FeaturedImage),
                        uploadResult.ErrorMessage!);
                    await LoadCategoryDataAsync(
                        model.CategoryId,
                        model.SubCategoryId);
                    return PartialView(
                        "_CreatePartial",
                        model);
                }
                featuredImagePath =
                    uploadResult.FilePath;
            }
            // =====================================================
            // CREATE POST
            // =====================================================
            var post =
                new Post
                {
                    Title =
                        model.Title.Trim(),
                    ShortDescription =
                        CleanOptionalValue(
                            model.ShortDescription),
                    Content =
                        CleanOptionalValue(
                            model.Content),
                    FeaturedImagePath =
                        featuredImagePath,
                    PostDate =
                        model.PostDate,
                    DisplayOrder =
                        model.DisplayOrder,
                    IsPublished =
                        model.IsPublished,
                    CategoryId =
                        model.CategoryId,
                    SubCategoryId =
                        model.SubCategoryId,
                    CreatedBy =
                        string.IsNullOrWhiteSpace(
                            user.FullName)
                            ? user.UserName ??
                              "Administrator"
                            : user.FullName,
                    CreatedAt =
                        DateTime.UtcNow
                };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            // =====================================================
            // GALLERY IMAGES
            // =====================================================
            var galleryFiles =
                new[]
                {
                    model.GalleryImage1,
                    model.GalleryImage2,
                    model.GalleryImage3
                };
            var uploadedGalleryPaths =
                new List<string>();
            try
            {
                for (int i = 0;
                     i < galleryFiles.Length;
                     i++)
                {
                    var file =
                        galleryFiles[i];
                    if (file == null)
                    {
                        continue;
                    }
                    var uploadResult =
                        await SaveGalleryImageAsync(
                            file);
                    if (!uploadResult.Success)
                    {
                        foreach (
                            var uploadedPath
                            in uploadedGalleryPaths)
                        {
                            DeletePostImageFile(
                                uploadedPath);
                        }
                        DeleteFeaturedImage(
                            post.FeaturedImagePath);
                        _context.Posts.Remove(post);
                        await _context.SaveChangesAsync();
                        ModelState.AddModelError(
                            GetGalleryFieldName(i),
                            uploadResult.ErrorMessage!);
                        await LoadCategoryDataAsync(
                            model.CategoryId,
                            model.SubCategoryId);
                        return PartialView(
                            "_CreatePartial",
                            model);
                    }
                    var imagePath =
                        uploadResult.FilePath!;
                    uploadedGalleryPaths.Add(
                        imagePath);
                    _context.PostImages.Add(
                        new PostImage
                        {
                            PostId =
                                post.Id,
                            ImagePath =
                                imagePath,
                            DisplayOrder =
                                i + 1,
                            CreatedAt =
                                DateTime.UtcNow
                        });
                }
                await _context.SaveChangesAsync();
            }
            catch
            {
                foreach (
                    var uploadedPath
                    in uploadedGalleryPaths)
                {
                    DeletePostImageFile(
                        uploadedPath);
                }
                DeleteFeaturedImage(
                    post.FeaturedImagePath);
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to save the post gallery images.");
                await LoadCategoryDataAsync(
                    model.CategoryId,
                    model.SubCategoryId);
                return PartialView(
                    "_CreatePartial",
                    model);
            }
            return Json(new
            {
                success = true,
                message =
                    "Post created successfully.",
                id = post.Id
            });
        }
        // =========================================================
        // EDIT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var post =
                await _context.Posts
                    .AsNoTracking()
                    .Include(x => x.Images)
                    .Include(x => x.Category)
                    .Include(x => x.SubCategory)
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            var gallery =
                post.Images
                    .OrderBy(x =>
                        x.DisplayOrder)
                    .ThenBy(x => x.Id)
                    .Take(3)
                    .ToList();
            var model =
                new PostEditViewModel
                {
                    Id =
                        post.Id,
                    Title =
                        post.Title,
                    ShortDescription =
                        post.ShortDescription,
                    Content =
                        post.Content,
                    CurrentFeaturedImagePath =
                        post.FeaturedImagePath,
                    PostDate =
                        post.PostDate,
                    DisplayOrder =
                        post.DisplayOrder,
                    IsPublished =
                        post.IsPublished,
                    CategoryId =
                        post.CategoryId,
                    SubCategoryId =
                        post.SubCategoryId,
                    CategoryName =
                        post.Category?.Name,
                    SubCategoryName =
                        post.SubCategory?.Name,
                    CreatedBy =
                        post.CreatedBy,
                    CreatedAt =
                        post.CreatedAt,
                    UpdatedAt =
                        post.UpdatedAt
                };
            PopulateGalleryModel(
                model,
                gallery);
            await LoadCategoryDataAsync(
                model.CategoryId,
                model.SubCategoryId);
            return PartialView(
                "_EditPartial",
                model);
        }
        // =========================================================
        // EDIT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            PostEditViewModel model)
        {
            var post =
                await _context.Posts
                    .Include(x => x.Images)
                    .Include(x => x.Category)
                    .Include(x => x.SubCategory)
                    .FirstOrDefaultAsync(
                        x => x.Id == model.Id);
            if (post == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Post not found."
                });
            }
            if (!ModelState.IsValid)
            {
                model.CurrentFeaturedImagePath =
                    post.FeaturedImagePath;
                model.CategoryName =
                    post.Category?.Name;
                model.SubCategoryName =
                    post.SubCategory?.Name;
                PopulateGalleryModel(
                    model,
                    post.Images
                        .OrderBy(x =>
                            x.DisplayOrder)
                        .ThenBy(x => x.Id)
                        .Take(3)
                        .ToList());
                await LoadCategoryDataAsync(
                    model.CategoryId,
                    model.SubCategoryId);
                return PartialView(
                    "_EditPartial",
                    model);
            }
            if (!await ValidateCategorySelectionAsync(
                    model.CategoryId,
                    model.SubCategoryId))
            {
                model.CurrentFeaturedImagePath =
                    post.FeaturedImagePath;
                PopulateGalleryModel(
                    model,
                    post.Images
                        .OrderBy(x =>
                            x.DisplayOrder)
                        .ThenBy(x => x.Id)
                        .Take(3)
                        .ToList());
                await LoadCategoryDataAsync(
                    model.CategoryId,
                    model.SubCategoryId);
                return PartialView(
                    "_EditPartial",
                    model);
            }
            // =====================================================
            // FEATURED IMAGE
            // =====================================================
            if (model.FeaturedImage != null)
            {
                var uploadResult =
                    await SaveFeaturedImageAsync(
                        model.FeaturedImage);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError(
                        nameof(model.FeaturedImage),
                        uploadResult.ErrorMessage!);
                    model.CurrentFeaturedImagePath =
                        post.FeaturedImagePath;
                    PopulateGalleryModel(
                        model,
                        post.Images
                            .OrderBy(x =>
                                x.DisplayOrder)
                            .ThenBy(x => x.Id)
                            .Take(3)
                            .ToList());
                    await LoadCategoryDataAsync(
                        model.CategoryId,
                        model.SubCategoryId);
                    return PartialView(
                        "_EditPartial",
                        model);
                }
                DeleteFeaturedImage(
                    post.FeaturedImagePath);
                post.FeaturedImagePath =
                    uploadResult.FilePath;
            }
            // =====================================================
            // UPDATE POST
            // =====================================================
            post.Title =
                model.Title.Trim();
            post.ShortDescription =
                CleanOptionalValue(
                    model.ShortDescription);
            post.Content =
                CleanOptionalValue(
                    model.Content);
            post.PostDate =
                model.PostDate;
            post.DisplayOrder =
                model.DisplayOrder;
            post.IsPublished =
                model.IsPublished;
            post.CategoryId =
                model.CategoryId;
            post.SubCategoryId =
                model.SubCategoryId;
            post.UpdatedAt =
                DateTime.UtcNow;
            // =====================================================
            // UPDATE GALLERY
            // =====================================================
            var galleryFiles =
                new[]
                {
                    model.GalleryImage1,
                    model.GalleryImage2,
                    model.GalleryImage3
                };
            var removeFlags =
                new[]
                {
                    model.RemoveGalleryImage1,
                    model.RemoveGalleryImage2,
                    model.RemoveGalleryImage3
                };
            for (int i = 0;
                 i < 3;
                 i++)
            {
                var displayOrder =
                    i + 1;
                var existingImage =
                    post.Images
                        .FirstOrDefault(
                            x =>
                                x.DisplayOrder ==
                                displayOrder);
                var newFile =
                    galleryFiles[i];
                var removeRequested =
                    removeFlags[i];
                // =================================================
                // NEW IMAGE / REPLACEMENT
                // =================================================
                if (newFile != null)
                {
                    var uploadResult =
                        await SaveGalleryImageAsync(
                            newFile);
                    if (!uploadResult.Success)
                    {
                        ModelState.AddModelError(
                            GetGalleryFieldName(i),
                            uploadResult.ErrorMessage!);
                        model.CurrentFeaturedImagePath =
                            post.FeaturedImagePath;
                        PopulateGalleryModel(
                            model,
                            post.Images
                                .OrderBy(x =>
                                    x.DisplayOrder)
                                .ThenBy(x => x.Id)
                                .Take(3)
                                .ToList());
                        await LoadCategoryDataAsync(
                            model.CategoryId,
                            model.SubCategoryId);
                        return PartialView(
                            "_EditPartial",
                            model);
                    }
                    if (existingImage != null)
                    {
                        DeletePostImageFile(
                            existingImage.ImagePath);
                        existingImage.ImagePath =
                            uploadResult.FilePath!;
                        existingImage.DisplayOrder =
                            displayOrder;
                    }
                    else
                    {
                        _context.PostImages.Add(
                            new PostImage
                            {
                                PostId =
                                    post.Id,
                                ImagePath =
                                    uploadResult.FilePath!,
                                DisplayOrder =
                                    displayOrder,
                                CreatedAt =
                                    DateTime.UtcNow
                            });
                    }
                    continue;
                }
                // =================================================
                // REMOVE EXISTING IMAGE
                // =================================================
                if (removeRequested &&
                    existingImage != null)
                {
                    DeletePostImageFile(
                        existingImage.ImagePath);
                    _context.PostImages.Remove(
                        existingImage);
                }
            }
            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message =
                    "Post updated successfully.",
                id = post.Id
            });
        }
        // =========================================================
        // DETAILS - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var post =
                await _context.Posts
                    .AsNoTracking()
                    .Include(x => x.Images)
                    .Include(x => x.Category)
                    .Include(x => x.SubCategory)
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            var model =
                new PostEditViewModel
                {
                    Id =
                        post.Id,
                    Title =
                        post.Title,
                    ShortDescription =
                        post.ShortDescription,
                    Content =
                        post.Content,
                    CurrentFeaturedImagePath =
                        post.FeaturedImagePath,
                    PostDate =
                        post.PostDate,
                    DisplayOrder =
                        post.DisplayOrder,
                    IsPublished =
                        post.IsPublished,
                    CategoryId =
                        post.CategoryId,
                    SubCategoryId =
                        post.SubCategoryId,
                    CategoryName =
                        post.Category?.Name,
                    SubCategoryName =
                        post.SubCategory?.Name,
                    CreatedBy =
                        post.CreatedBy,
                    CreatedAt =
                        post.CreatedAt,
                    UpdatedAt =
                        post.UpdatedAt
                };
            PopulateGalleryModel(
                model,
                post.Images
                    .OrderBy(x =>
                        x.DisplayOrder)
                    .ThenBy(x => x.Id)
                    .Take(3)
                    .ToList());
            return PartialView(
                "_DetailsPartial",
                model);
        }
        // =========================================================
        // DELETE
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            var post =
                await _context.Posts
                    .Include(x => x.Images)
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (post == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Post not found."
                });
            }
            // Delete featured image file
            DeleteFeaturedImage(
                post.FeaturedImagePath);
            // Delete gallery image files
            foreach (
                var image in post.Images)
            {
                DeletePostImageFile(
                    image.ImagePath);
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message =
                    "Post deleted successfully.",
                id
            });
        }
        // =========================================================
        // TOGGLE PUBLISH
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(
            int id)
        {
            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(
                        x => x.Id == id);
            if (post == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Post not found."
                });
            }
            post.IsPublished =
                !post.IsPublished;
            post.UpdatedAt =
                DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message =
                    post.IsPublished
                        ? "Post published successfully."
                        : "Post unpublished successfully.",
                isPublished =
                    post.IsPublished,
                id =
                    post.Id
            });
        }
        // =========================================================
        // BULK ACTION
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction(
            [FromForm] int[] ids,
            [FromForm] string action)
        {
            if (ids == null ||
                ids.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Please select at least one post."
                });
            }
            action =
                action?
                    .Trim()
                    .ToLowerInvariant()
                ?? string.Empty;
            if (action != "publish" &&
                action != "unpublish" &&
                action != "delete")
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Invalid bulk action."
                });
            }
            var distinctIds =
                ids
                    .Distinct()
                    .ToArray();
            var posts =
                await _context.Posts
                    .Include(x => x.Images)
                    .Where(x =>
                        distinctIds.Contains(
                            x.Id))
                    .ToListAsync();
            if (posts.Count == 0)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "No selected posts were found."
                });
            }
            // =====================================================
            // BULK DELETE
            // =====================================================
            if (action == "delete")
            {
                foreach (var post in posts)
                {
                    DeleteFeaturedImage(
                        post.FeaturedImagePath);
                    foreach (
                        var image in post.Images)
                    {
                        DeletePostImageFile(
                            image.ImagePath);
                    }
                }
                _context.Posts.RemoveRange(
                    posts);
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message =
                        $"{posts.Count} post(s) deleted successfully."
                });
            }
            // =====================================================
            // BULK PUBLISH / UNPUBLISH
            // =====================================================
            var publish =
                action == "publish";
            foreach (var post in posts)
            {
                post.IsPublished =
                    publish;
                post.UpdatedAt =
                    DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message =
                    publish
                        ? $"{posts.Count} post(s) published successfully."
                        : $"{posts.Count} post(s) unpublished successfully."
            });
        }
        // =========================================================
        // FEATURED IMAGE UPLOAD
        // =========================================================
        private async Task<UploadResult>
            SaveFeaturedImageAsync(
                IFormFile file)
        {
            if (file.Length <= 0)
            {
                return UploadResult.Fail(
                    "Please select a valid image.");
            }
            const long maxFileSize =
                2 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return UploadResult.Fail(
                    "Featured image cannot exceed 2 MB.");
            }
            var allowedExtensions =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };
            var extension =
                Path.GetExtension(
                    file.FileName);
            if (string.IsNullOrWhiteSpace(
                    extension) ||
                !allowedExtensions.Contains(
                    extension))
            {
                return UploadResult.Fail(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }
            var allowedContentTypes =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    "image/jpeg",
                    "image/png",
                    "image/webp"
                };
            if (!allowedContentTypes.Contains(
                    file.ContentType))
            {
                return UploadResult.Fail(
                    "Invalid image file type.");
            }
            var uploadDirectory =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "posts");
            Directory.CreateDirectory(
                uploadDirectory);
            var fileName =
                $"{Guid.NewGuid():N}" +
                extension.ToLowerInvariant();
            var physicalPath =
                Path.Combine(
                    uploadDirectory,
                    fileName);
            await using (
                var stream =
                    new FileStream(
                        physicalPath,
                        FileMode.CreateNew))
            {
                await file.CopyToAsync(
                    stream);
            }
            return UploadResult.Ok(
                $"/uploads/posts/{fileName}");
        }
        // =========================================================
        // GALLERY IMAGE UPLOAD
        // =========================================================
        private async Task<UploadResult>
            SaveGalleryImageAsync(
                IFormFile file)
        {
            if (file.Length <= 0)
            {
                return UploadResult.Fail(
                    "Please select a valid gallery image.");
            }
            const long maxFileSize =
                2 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return UploadResult.Fail(
                    "Gallery image cannot exceed 2 MB.");
            }
            var allowedExtensions =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };
            var extension =
                Path.GetExtension(
                    file.FileName);
            if (string.IsNullOrWhiteSpace(
                    extension) ||
                !allowedExtensions.Contains(
                    extension))
            {
                return UploadResult.Fail(
                    "Only JPG, JPEG, PNG and WEBP gallery images are allowed.");
            }
            var allowedContentTypes =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    "image/jpeg",
                    "image/png",
                    "image/webp"
                };
            if (!allowedContentTypes.Contains(
                    file.ContentType))
            {
                return UploadResult.Fail(
                    "Invalid gallery image file type.");
            }
            var uploadDirectory =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "posts");
            Directory.CreateDirectory(
                uploadDirectory);
            var fileName =
                $"{Guid.NewGuid():N}" +
                extension.ToLowerInvariant();
            var physicalPath =
                Path.Combine(
                    uploadDirectory,
                    fileName);
            await using (
                var stream =
                    new FileStream(
                        physicalPath,
                        FileMode.CreateNew))
            {
                await file.CopyToAsync(
                    stream);
            }
            return UploadResult.Ok(
                $"/uploads/posts/{fileName}");
        }
        // =========================================================
        // DELETE FEATURED IMAGE FILE
        // =========================================================
        private void DeleteFeaturedImage(
            string? imagePath)
        {
            DeletePostImageFile(
                imagePath);
        }
        // =========================================================
        // DELETE POST IMAGE FILE
        // =========================================================
        private void DeletePostImageFile(
            string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(
                    imagePath))
            {
                return;
            }
            try
            {
                var fileName =
                    Path.GetFileName(
                        imagePath);
                if (string.IsNullOrWhiteSpace(
                        fileName))
                {
                    return;
                }
                var physicalPath =
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "posts",
                        fileName);
                if (System.IO.File.Exists(
                        physicalPath))
                {
                    System.IO.File.Delete(
                        physicalPath);
                }
            }
            catch
            {
                // Image cleanup must not
                // break post operations.
            }
        }
        // =========================================================
        // POPULATE GALLERY VIEWMODEL
        // =========================================================
        private static void PopulateGalleryModel(
            PostEditViewModel model,
            IEnumerable<PostImage> images)
        {
            var gallery =
                images
                    .OrderBy(x =>
                        x.DisplayOrder)
                    .ThenBy(x => x.Id)
                    .Take(3)
                    .ToList();
            var image1 =
                gallery.FirstOrDefault(
                    x => x.DisplayOrder == 1);
            var image2 =
                gallery.FirstOrDefault(
                    x => x.DisplayOrder == 2);
            var image3 =
                gallery.FirstOrDefault(
                    x => x.DisplayOrder == 3);
            // =====================================================
            // PHOTO 1
            // =====================================================
            if (image1 != null)
            {
                model.GalleryImage1Id =
                    image1.Id;
                model.GalleryImage1Path =
                    image1.ImagePath;
            }
            // =====================================================
            // PHOTO 2
            // =====================================================
            if (image2 != null)
            {
                model.GalleryImage2Id =
                    image2.Id;
                model.GalleryImage2Path =
                    image2.ImagePath;
            }
            // =====================================================
            // PHOTO 3
            // =====================================================
            if (image3 != null)
            {
                model.GalleryImage3Id =
                    image3.Id;
                model.GalleryImage3Path =
                    image3.ImagePath;
            }
        }
        // =========================================================
        // GALLERY FIELD NAME
        // =========================================================
        private static string GetGalleryFieldName(
            int index)
        {
            return index switch
            {
                0 => nameof(
                    PostCreateViewModel.GalleryImage1),
                1 => nameof(
                    PostCreateViewModel.GalleryImage2),
                _ => nameof(
                    PostCreateViewModel.GalleryImage3)
            };
        }
        // =========================================================
        // CLEAN OPTIONAL VALUE
        // =========================================================
        private static string?
            CleanOptionalValue(
                string? value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return null;
            }
            return value.Trim();
        }
        // =========================================================
        // UPLOAD RESULT
        // =========================================================
        private sealed class UploadResult
        {
            public bool Success
            {
                get;
                private set;
            }
            public string? FilePath
            {
                get;
                private set;
            }
            public string? ErrorMessage
            {
                get;
                private set;
            }
            public static UploadResult Ok(
                string filePath)
            {
                return new UploadResult
                {
                    Success = true,
                    FilePath = filePath
                };
            }
            public static UploadResult Fail(
                string message)
            {
                return new UploadResult
                {
                    Success = false,
                    ErrorMessage = message
                };
            }
        }
        // =========================================================
        // CATEGORY HELPERS
        // =========================================================
        private async Task<List<PostCategory>> GetActiveCategoriesAsync()
        {
            return await _context.PostCategories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }
        private async Task<List<PostSubCategory>> GetAllActiveSubCategoriesAsync()
        {
            return await _context.PostSubCategories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.PostCategoryId)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }
        private async Task<List<PostSubCategory>> GetActiveSubCategoriesAsync(
            int categoryId)
        {
            return await _context.PostSubCategories
                .AsNoTracking()
                .Where(x =>
                    x.PostCategoryId == categoryId &&
                    x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }
        private async Task LoadCategoryDataAsync(
            int? selectedCategoryId = null,
            int? selectedSubCategoryId = null)
        {
            ViewBag.PostCategories =
                await GetActiveCategoriesAsync();
            if (selectedCategoryId.HasValue &&
                selectedCategoryId.Value > 0)
            {
                ViewBag.PostSubCategories =
                    await GetActiveSubCategoriesAsync(
                        selectedCategoryId.Value);
            }
            else
            {
                ViewBag.PostSubCategories =
                    new List<PostSubCategory>();
            }
            ViewBag.SelectedCategoryId =
                selectedCategoryId;
            ViewBag.SelectedSubCategoryId =
                selectedSubCategoryId;
        }
    }
}