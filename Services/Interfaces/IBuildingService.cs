using System.Collections.Generic;
using System.Threading.Tasks;
using YoneticiOtomasyonu.Models;

namespace YoneticiOtomasyonu.Services.Interfaces
{
    public interface IBuildingService
    {
        Task<(bool Success, string Message)> CreateBuildingAsync(Building building, string creatorUserId);
        Task<bool> IsBuildingExistsAsync(string buildingName, string address);
        Task<bool> AssignRoleToUserAsync(string assignerUserId, int userProfileId, int buildingId, string role);
        Task<string> GetUserRoleInBuildingAsync(int userProfileId, int buildingId);
        Task<List<(Building Building, string Role)>> GetUserBuildingsWithRolesAsync(int userProfileId);
        Task<bool> IsBuildingAdminAsync(string identityUserId, int buildingId);
        Task<List<Building>> GetAllBuildingsAsync();
        Task<Building> GetBuildingByIdAsync(int id);
        Task<List<UserBuildingRole>> GetBuildingManagersAsync(int buildingId);
        Task<bool> UserHasAccessToBuildingAsync(string userId, int buildingId);

    }
}