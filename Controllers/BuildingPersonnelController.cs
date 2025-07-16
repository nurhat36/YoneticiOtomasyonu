using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models.ViewModels;

namespace YoneticiOtomasyonu.Controllers
{
    [Authorize(Policy = "BuildingAccess")]
    [Route("Buildings/{buildingId:int}/[controller]")]
    public class BuildingPersonnelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BuildingPersonnelController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int buildingId)
        {
            var personnels = await _context.BuildingPersonnel
                .Where(p => p.BuildingId == buildingId)
                .Include(p => p.ApplicationUser)
                .ToListAsync();
            ViewBag.BuildingId = buildingId;
            return View(personnels);
        }
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int buildingId)
        {
            var users = await _userManager.Users.ToListAsync(); // veya filtrele
            var vm = new BuildingPersonnelFormViewModel
            {
                Personnel = new BuildingPersonnel { BuildingId = buildingId },
                Users = users
            };
            return View(vm);
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create(BuildingPersonnel personnel)
        {
            personnel.EmploymentStartDate = DateTime.Now;
            personnel.IsActive = true;

            _context.Add(personnel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { buildingId = personnel.BuildingId });
        }
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int buildingID,int id)
        {
            var personnel = await _context.BuildingPersonnel
         .Include(p => p.ApplicationUser)
         .FirstOrDefaultAsync(p => p.Id == id);

            if (personnel == null)
                return NotFound();

            var vm = new BuildingPersonnelFormViewModel
            {
                Personnel = personnel,
                Users = null // gerek yok
            };
            ViewBag.BuildingId = buildingID;

            return View(vm);
        }


        [HttpPost("{id}/Edit")]
        public async Task<IActionResult> Edit(int buildingId,int id, BuildingPersonnel personnel)
        {
            var existing = await _context.BuildingPersonnel.FindAsync(id);
            Console.WriteLine("Editing personnel with ID: " + id);
            if (existing == null)
            {
                Console.WriteLine("Personnel not found for ID: " + id);
                return NotFound();
            }

            existing.DutyType = personnel.DutyType;
            existing.MonthlySalary = personnel.MonthlySalary;
            existing.Notes = personnel.Notes;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { buildingId = existing.BuildingId });
        }
        [HttpGet("{id}/Detail")]
        public async Task<IActionResult> Details(int buildingId,int id)
        {
            var personnel = await _context.BuildingPersonnel
                .Include(p => p.ApplicationUser)  // Personelin kullanıcı bilgisi için
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personnel == null)
                return NotFound();
            ViewBag.BuildingId = buildingId;

            return View(personnel);
        }
        
        public async Task<IActionResult> Deactivate(int buildingId,int id)
        {
            var personnel = await _context.BuildingPersonnel.FindAsync(id);
            if (personnel == null)
            {
                return NotFound();
            }

            personnel.IsActive = false;
            personnel.EmploymentEndDate = DateTime.Now;
            ViewBag.BuildingId = buildingId;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { buildingId = personnel.BuildingId });
        }
    }
}
