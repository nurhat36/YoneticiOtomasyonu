using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
namespace YoneticiOtomasyonu.Controllers { 
public class WorkTaskController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public WorkTaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> MyTasks()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var tasks = await _context.WorkTasks
            .Include(t => t.Complaint)
            .Include(t => t.Building)
            .Where(t => t.AssignedToId == user.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tasks);
    }
        public async Task<IActionResult> Detail(int id)
        {
            var task = await _context.WorkTasks
                .Include(t => t.Complaint)
                    .ThenInclude(c => c.Images) // 📷 Görselleri de dahil et
                .Include(t => t.Building)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }
            var statusList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Beklemede", Value = "Beklemede" },
        new SelectListItem { Text = "Devam Ediyor", Value = "Devam Ediyor" },
        new SelectListItem { Text = "Tamamlandı", Value = "Tamamlandı" }
    };

            ViewBag.StatusList = new SelectList(statusList, "Value", "Text", task.Status);

            return View(task);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var task = await _context.WorkTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", new { id });
        }



    }
}