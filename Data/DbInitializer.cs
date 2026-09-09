using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();
            var roleManager =
                scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();
            var userManager =
                scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();
            // ==============================================
            // DATABASE
            // ==============================================
            await context.Database.MigrateAsync();
            // ==============================================
            // CREATE ROLES
            // ==============================================
            string[] roles =
            {
                UserRole.Administrator,
                UserRole.DonationVerifier,
                UserRole.PostManager
            };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            "Failed to create role " +
                            role +
                            ": " +
                            string.Join(
                                ", ",
                                roleResult.Errors.Select(
                                    x => x.Description)));
                    }
                }
            }
            // ==============================================
            // ADMIN ACCOUNT
            // ==============================================
            const string adminEmail =
                "admin@foundation.local";
            const string adminPassword =
                "Admin@12345";
            var admin =
                await userManager.FindByEmailAsync(
                    adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var createResult =
                    await userManager.CreateAsync(
                        admin,
                        adminPassword);
                if (!createResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to create administrator: " +
                        string.Join(
                            ", ",
                            createResult.Errors.Select(
                                x => x.Description)));
                }
            }
            else
            {
                // Make sure account is active
                admin.IsActive = true;
                admin.EmailConfirmed = true;
                await userManager.UpdateAsync(admin);
            }
            // ==============================================
            // ADMIN ROLE
            // ==============================================
            if (!await userManager.IsInRoleAsync(
                    admin,
                    UserRole.Administrator))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        admin,
                        UserRole.Administrator);
                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to assign administrator role: " +
                        string.Join(
                            ", ",
                            roleResult.Errors.Select(
                                x => x.Description)));
                }
            }
            // ==============================================
            // FOUNDATION SETTINGS
            // ==============================================
            if (!await context.FoundationSettings.AnyAsync())
            {
                context.FoundationSettings.Add(
                    new FoundationSetting
                    {
                        FoundationName =
                            "Your Foundation Name",
                        ReceiptFooter =
                            "Thank you for supporting our Foundation.",
                        UpdatedAt =
                            DateTime.UtcNow
                    });
                await context.SaveChangesAsync();
            }
        }
    }
}