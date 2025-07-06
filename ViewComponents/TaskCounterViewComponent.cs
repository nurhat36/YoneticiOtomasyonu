using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data; // DbContext'in olduğu namespace
using YoneticiOtomasyonu.Models;
using Microsoft.AspNetCore.Identity;

namespace YoneticiOtomasyonu.ViewComponents
{
    public class TaskCounterViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskCounterViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            int incompleteCount = 0;
            if (user != null)
            {
                incompleteCount = await _context.WorkTasks
                    .Where(x => x.AssignedToId == user.Id && x.Status != "Tamamlandı")
                    .CountAsync();
            }

            return View(incompleteCount);
        }
    }
}
