using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models;

namespace YoneticiOtomasyonu.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create roles if they don't exist
            string[] roleNames = { "SuperAdmin", "Yönetici", "Kiracı", "Görevli" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user
            var adminEmail = "admin@yoneticiotomasyonu.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = "5551234567", // Sample phone number
                    ProfileImageUrl = "/uploads/profile-images/6663f122-be96-4dbb-81b1-53e38824ed6c_20250626212920.jpg" // Default profile image URL
                };

                var createUser = await userManager.CreateAsync(adminUser, "Admin123!"); // Strong password

                if (createUser.Succeeded)
                {
                    // Assign SuperAdmin role to admin user
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");

                    // Create user profile for admin
                    var adminProfile = new UserProfile
                    {
                        IdentityUserId = adminUser.Id,
                        FullName = "Sistem Yöneticisi",
                        TCKN = "11111111111", // Sample TCKN
                        PhoneNumber2 = "5551112233",
                        RoleInBuilding = "SuperAdmin"
                    };

                    context.UserProfiles.Add(adminProfile);
                    await context.SaveChangesAsync();
                }
            }

            // Optionally create sample buildings
            if (!context.Buildings.Any())
            {
                var building = new Building
                {
                    Name = "Örnek Site",
                    Address = "Örnek Mah. Demo Sokak No:1 İstanbul",
                    Type = "Site",
                    FloorCount = 10,
                    UnitCount = 40,
                    CreatedAt = DateTime.Now,
                    Description = "Sistem tarafından oluşturulan örnek site",
                    CreatorUserId = adminUser?.Id
                };

                context.Buildings.Add(building);
                await context.SaveChangesAsync();

                // Assign admin as building admin if exists
                if (adminUser != null && context.UserProfiles.Any())
                {
                    var adminProfile = await context.UserProfiles
                        .FirstOrDefaultAsync(up => up.IdentityUserId == adminUser.Id);

                    if (adminProfile != null)
                    {
                        var userBuildingRole = new UserBuildingRole
                        {
                            UserProfileId = adminProfile.Id,
                            BuildingId = building.Id,
                            Role = "Yönetici",
                            IsPrimary = true,
                            AssignedByUserId = adminUser.Id,
                            AssignmentDate = DateTime.Now
                        };

                        context.UserBuildingRoles.Add(userBuildingRole);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}