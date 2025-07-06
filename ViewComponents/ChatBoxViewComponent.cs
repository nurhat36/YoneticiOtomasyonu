using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using YoneticiOtomasyonu.Models;
namespace YoneticiOtomasyonu.ViewComponents
{
    

    public class ChatBoxViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatBoxViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var profilePic = user?.ProfileImageUrl ?? "/images/default-profile.png";
            ViewData["ProfilePic"] = profilePic;
            ViewData["UserName"] = user?.UserName ?? "Misafir";

            // Eğer chat kutusuna başlangıçta ihtiyaç duyulan başka veriler varsa burada hazırla

            return View();
        }
    }

}
