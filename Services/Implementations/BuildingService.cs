using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models.ViewModels;
using YoneticiOtomasyonu.Services.Interfaces;

namespace YoneticiOtomasyonu.Services.Implementations
{
    public class BuildingService : IBuildingService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BuildingService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<UserBuildingRole>> GetBuildingManagersAsync(int buildingId)
        {
            return await _context.UserBuildingRoles
                .Include(ubr => ubr.UserProfile)
                .ThenInclude(up => up.IdentityUser)
                .Where(ubr => ubr.BuildingId == buildingId && ubr.Role == "Yönetici")
                .OrderByDescending(ubr => ubr.IsPrimary)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateBuildingAsync(Building building, string creatorUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Bina adı ve adres kontrolü
                if (await IsBuildingExistsAsync(building.Name, building.Address))
                {
                    return (false, "Bu isim ve adreste zaten bir bina kayıtlı");
                }

                // Binayı oluştur
                building.CreatorUserId = creatorUserId;
                building.CreatedAt = DateTime.Now;
                _context.Buildings.Add(building);
                await _context.SaveChangesAsync();

                // Oluşturan kullanıcıyı bul
                var creatorProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.IdentityUserId == creatorUserId);

                if (creatorProfile == null)
                {
                    return (false, "Kullanıcı profili bulunamadı");
                }

                // Oluşturan kullanıcıya otomatik yönetici rolü ata
                var adminRole = new UserBuildingRole
                {
                    UserProfileId = creatorProfile.Id,
                    BuildingId = building.Id,
                    Role = "Yönetici",
                    IsPrimary = true,
                    AssignedByUserId = creatorUserId,
                    AssignmentDate = DateTime.Now
                };

                _context.UserBuildingRoles.Add(adminRole);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Bina başarıyla oluşturuldu ve yönetici atandı");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Loglama yapılabilir
                return (false, $"Bina oluşturulurken hata: {ex.Message}");
            }
        }

        public async Task<bool> IsBuildingExistsAsync(string buildingName, string address)
        {
            return await _context.Buildings
                .AnyAsync(b =>
                    b.Name.ToLower() == buildingName.ToLower() &&
                    b.Address.ToLower() == address.ToLower());
        }

        public async Task<List<Building>> GetAllBuildingsAsync()
        {
            return await _context.Buildings
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Building> GetBuildingByIdAsync(int id)
        {
            return await _context.Buildings
                .Include(b => b.UserRoles)
                .ThenInclude(ubr => ubr.UserProfile)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // Diğer metodlar (önceden tanımladıklarımız)
        public async Task<bool> AssignRoleToUserAsync(string assignerUserId, int userProfileId, int buildingId, string role)
        {
            // Atayanın yetkilerini kontrol et
            var assigner = await _userManager.FindByIdAsync(assignerUserId);
            var isSuperAdmin = await _userManager.IsInRoleAsync(assigner, "SuperAdmin");

            if (!isSuperAdmin && !await IsBuildingAdminAsync(assignerUserId, buildingId))
            {
                return false;
            }

            // Rol atamasını yap
            var existingRole = await _context.UserBuildingRoles
                .FirstOrDefaultAsync(ubr => ubr.UserProfileId == userProfileId && ubr.BuildingId == buildingId);

            if (existingRole != null)
            {
                existingRole.Role = role;
                existingRole.AssignmentDate = DateTime.Now;
                existingRole.AssignedByUserId = assignerUserId;
            }
            else
            {
                var newRole = new UserBuildingRole
                {
                    UserProfileId = userProfileId,
                    BuildingId = buildingId,
                    Role = role,
                    AssignmentDate = DateTime.Now,
                    AssignedByUserId = assignerUserId,
                    IsPrimary = false
                };
                _context.UserBuildingRoles.Add(newRole);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetUserRoleInBuildingAsync(int userProfileId, int buildingId)
        {
            var role = await _context.UserBuildingRoles
                .Where(ubr => ubr.UserProfileId == userProfileId && ubr.BuildingId == buildingId)
                .Select(ubr => ubr.Role)
                .FirstOrDefaultAsync();

            return role ?? "Rol Yok";
        }

        public async Task<List<(Building Building, string Role)>> GetUserBuildingsWithRolesAsync(int userProfileId)
        {
            var results = await _context.UserBuildingRoles
                .Where(ubr => ubr.UserProfileId == userProfileId)
                .Include(ubr => ubr.Building)
                .Select(ubr => new { ubr.Building, ubr.Role })
                .ToListAsync();

            return results.Select(x => (x.Building, x.Role)).ToList();
        }

        public async Task<bool> IsBuildingAdminAsync(string identityUserId, int buildingId)
        {
            var userProfileId = await _context.UserProfiles
                .Where(up => up.IdentityUserId == identityUserId)
                .Select(up => up.Id)
                .FirstOrDefaultAsync();

            if (userProfileId == 0) return false;

            var role = await GetUserRoleInBuildingAsync(userProfileId, buildingId);
            return role == "Yönetici";
        }
        public async Task<List<BuildingWithRoleViewModel>> GetBuildingsWithUserRolesAsync(string userId)
        {
            var userProfileId = await _context.UserProfiles
                .Where(up => up.IdentityUserId == userId)
                .Select(up => up.Id)
                .FirstOrDefaultAsync();

            return await _context.UserBuildingRoles
                .Where(ubr => ubr.UserProfileId == userProfileId)
                .Include(ubr => ubr.Building)
                .Select(ubr => new BuildingWithRoleViewModel
                {
                    Building = ubr.Building,
                    Role = ubr.Role
                })
                .ToListAsync();
        }

        public async Task<BuildingWithRoleViewModel> GetBuildingWithUserRoleAsync(int buildingId, string userId)
        {
            var userProfileId = await _context.UserProfiles
                .Where(up => up.IdentityUserId == userId)
                .Select(up => up.Id)
                .FirstOrDefaultAsync();

            var buildingRole = await _context.UserBuildingRoles
                .Include(ubr => ubr.Building)
                .FirstOrDefaultAsync(ubr => ubr.BuildingId == buildingId &&
                                          ubr.UserProfileId == userProfileId);

            return buildingRole == null
                ? new BuildingWithRoleViewModel
                {
                    Building = await _context.Buildings.FindAsync(buildingId),
                    Role = "Rol Yok"
                }
                : new BuildingWithRoleViewModel
                {
                    Building = buildingRole.Building,
                    Role = buildingRole.Role
                };
        }
    }
}