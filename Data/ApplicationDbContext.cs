using FoundationDonationSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // =========================================================
        // DB SETS
        // =========================================================
        public DbSet<Donation> Donations { get; set; }
        public DbSet<DonationSlip> DonationSlips { get; set; }
        public DbSet<PaymentAccount> PaymentAccounts { get; set; }
        public DbSet<FoundationSetting> FoundationSettings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<PostSubCategory> PostSubCategories { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; } = null!;
        public DbSet<Grievance> Grievances { get; set; } = null!;
        // =========================================================
        // MODEL CONFIGURATION
        // =========================================================
        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // =====================================================
            // DONATION
            // =====================================================
            builder.Entity<Donation>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);
            builder.Entity<Donation>()
                .HasIndex(x => x.DonationNumber)
                .IsUnique();
            builder.Entity<Donation>()
                .HasIndex(x => x.TransactionId);
            builder.Entity<Donation>()
                .HasIndex(x => x.Status);
            builder.Entity<Donation>()
                .HasIndex(x => x.CreatedAt);
            // =====================================================
            // DONATION SLIP
            // =====================================================
            builder.Entity<DonationSlip>()
                .HasIndex(x => x.SlipNumber)
                .IsUnique();
            builder.Entity<DonationSlip>()
                .HasIndex(x => x.DonationId)
                .IsUnique();
            builder.Entity<DonationSlip>()
                .HasOne(x => x.Donation)
                .WithMany()
                .HasForeignKey(x => x.DonationId)
                .OnDelete(DeleteBehavior.Restrict);
            // =====================================================
            // PAYMENT ACCOUNT
            // =====================================================
            builder.Entity<PaymentAccount>()
                .HasIndex(x => x.IsActive);
            // =====================================================
            // FOUNDATION SETTINGS
            // =====================================================
            builder.Entity<FoundationSetting>()
                .HasIndex(x => x.FoundationName);
            // =====================================================
            // POST
            // =====================================================
            builder.Entity<Post>()
                .HasIndex(x => x.IsPublished);
            builder.Entity<Post>()
                .HasIndex(x => x.PostDate);
            builder.Entity<Post>()
                .HasIndex(x => x.DisplayOrder);
            builder.Entity<Post>()
                .HasIndex(x => new
                {
                    x.IsPublished,
                    x.PostDate
                });
            // =====================================================
            // POST CATEGORY
            // =====================================================
            builder.Entity<PostCategory>()
                .HasIndex(x => x.Name)
                .IsUnique();
            builder.Entity<PostCategory>()
                .HasIndex(x => new
                {
                    x.IsActive,
                    x.DisplayOrder
                });
            // =====================================================
            // POST SUBCATEGORY
            // =====================================================
            builder.Entity<PostSubCategory>()
                .HasIndex(x => new
                {
                    x.PostCategoryId,
                    x.Name
                })
                .IsUnique();
            builder.Entity<PostSubCategory>()
                .HasIndex(x => new
                {
                    x.PostCategoryId,
                    x.IsActive,
                    x.DisplayOrder
                });
            // =====================================================
            // CATEGORY → SUBCATEGORY
            // =====================================================
            builder.Entity<PostSubCategory>()
                .HasOne(x => x.PostCategory)
                .WithMany(x => x.SubCategories)
                .HasForeignKey(x => x.PostCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            // =====================================================
            // POST → CATEGORY
            // =====================================================
            builder.Entity<Post>()
                .HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            // =====================================================
            // POST → SUBCATEGORY
            // =====================================================
            builder.Entity<Post>()
                .HasOne(x => x.SubCategory)
                .WithMany()
                .HasForeignKey(x => x.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            // =====================================================
            // POST IMAGES
            // =====================================================
            builder.Entity<PostImage>()
                .HasIndex(x => new
                {
                    x.PostId,
                    x.DisplayOrder
                });
            builder.Entity<PostImage>()
                .HasOne(x => x.Post)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            // =====================================================
            // DEFAULT POST CATEGORIES
            // =====================================================
            var activitiesCategoryId = 1;
            var announcementCategoryId = 2;
            var newsCategoryId = 3;
            builder.Entity<PostCategory>().HasData(
                new PostCategory
                {
                    Id = activitiesCategoryId,
                    Name = "Activities",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                },
                new PostCategory
                {
                    Id = announcementCategoryId,
                    Name = "Announcement",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                },
                new PostCategory
                {
                    Id = newsCategoryId,
                    Name = "News",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                }
            );
            // =====================================================
            // DEFAULT ACTIVITIES SUBCATEGORIES
            // =====================================================
            builder.Entity<PostSubCategory>().HasData(
                new PostSubCategory
                {
                    Id = 1,
                    PostCategoryId = activitiesCategoryId,
                    Name = "Education & Learning",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                },
                new PostSubCategory
                {
                    Id = 2,
                    PostCategoryId = activitiesCategoryId,
                    Name = "Health & Wellbeing",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                },
                new PostSubCategory
                {
                    Id = 3,
                    PostCategoryId = activitiesCategoryId,
                    Name = "Community Support",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                },
                new PostSubCategory
                {
                    Id = 4,
                    PostCategoryId = activitiesCategoryId,
                    Name = "Livelihood & Opportunity",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                },
                new PostSubCategory
                {
                    Id = 5,
                    PostCategoryId = activitiesCategoryId,
                    Name = "Relief & Assistance",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                },
                new PostSubCategory
                {
                    Id = 6,
                    PostCategoryId = activitiesCategoryId,
                    Name = "Social Development",
                    DisplayOrder = 6,
                    IsActive = true,
                    CreatedAt = new DateTime(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                }
            );
        }
    }
}