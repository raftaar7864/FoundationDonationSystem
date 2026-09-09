using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using FoundationDonationSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
var builder = WebApplication.CreateBuilder(args);
// =====================================================
// DATABASE
// =====================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));
// =====================================================
// IDENTITY
// =====================================================
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // -------------------------------------------------
        // Password
        // -------------------------------------------------
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        // -------------------------------------------------
        // User
        // -------------------------------------------------
        options.User.RequireUniqueEmail = true;
        // -------------------------------------------------
        // Lockout
        // -------------------------------------------------
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
// =====================================================
// COOKIE CONFIGURATION
// =====================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath =
        "/Account/AccessDenied";
    options.ExpireTimeSpan =
        TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
// =====================================================
// MVC
// =====================================================
builder.Services.AddControllersWithViews();
// =====================================================
// APPLICATION SERVICES
// =====================================================
builder.Services.AddScoped<IDonationService,
    DonationService>();
builder.Services.AddScoped<IDonationSlipService,
    DonationSlipService>();
builder.Services.AddScoped<IReferenceNumberService,
    ReferenceNumberService>();
builder.Services.AddScoped<IFileUploadService,
    FileUploadService>();
builder.Services.AddScoped<IQrCodeService,
    QrCodeService>();
builder.Services.AddScoped<INotificationService,
    NotificationService>();
builder.Services.AddScoped<IFoundationSettingsService, 
    FoundationSettingsService>();
// =====================================================
// BUILD APPLICATION
// =====================================================
var app = builder.Build();
// =====================================================
// HTTP PIPELINE
// =====================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// =====================================================
// ATTRIBUTE ROUTING
// =====================================================
//
// Required for controllers/actions using [Route(...)]
// such as:
//
// /admin/paymentaccounts
// /admin/paymentaccounts/create
// /admin/paymentaccounts/edit/1
//
// =====================================================
app.MapControllers();
// =====================================================
// CONVENTIONAL ROUTING - AREAS
// =====================================================
app.MapControllerRoute(
    name: "areas",
    pattern:
        "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
// =====================================================
// CONVENTIONAL ROUTING - DEFAULT
// =====================================================
app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");
// =====================================================
// DATABASE INITIALIZATION
// =====================================================
await DbInitializer.InitializeAsync(
    app.Services);
QuestPDF.Settings.License =
    LicenseType.Community;
// =====================================================
// RUN
// =====================================================
app.Run();