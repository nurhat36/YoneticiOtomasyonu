using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using YoneticiOtomasyonu.Data;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Security;
namespace YoneticiOtomasyonu.Security { 
public class BuildingAccessHandler : AuthorizationHandler<BuildingAccessRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BuildingAccessHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BuildingAccessRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var routeData = httpContext.GetRouteData();

        // buildingId veya id parametresini al
        var buildingIdStr = routeData.Values["buildingId"]?.ToString();
        if (string.IsNullOrEmpty(buildingIdStr))
            buildingIdStr = routeData.Values["id"]?.ToString();

        if (!int.TryParse(buildingIdStr, out var buildingId))
        {
            context.Fail();
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        // UserProfile
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);
        if (userProfile == null)
        {
            context.Fail();
            return;
        }

        var hasAccess = await _context.UserBuildingRoles.AnyAsync(r => r.UserProfileId == userProfile.Id && r.BuildingId == buildingId);
        if (hasAccess)
        {
            context.Succeed(requirement);
        }
        else
        {
            // Yetki yok: Direkt redirect
            httpContext.Response.Redirect("/Buildings");
            context.Fail();
        }
    }
}
}