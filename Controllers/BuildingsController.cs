using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models.ViewModels;
using YoneticiOtomasyonu.Services.Interfaces;

namespace YoneticiOtomasyonu.Controllers
{
    [Authorize]
    [Route("Buildings/{buildingId:int}/[action]")]
    public class BuildingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBuildingService _buildingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BuildingsController(
            ApplicationDbContext context,
            IBuildingService buildingService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _buildingService = buildingService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [Route("/Buildings")]
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == currentUserId);

            if (userProfile == null)
                return NotFound();

            var buildingsWithRoles = await _context.UserBuildingRoles
                .Where(ubr => ubr.UserProfileId == userProfile.Id)
                .Include(ubr => ubr.Building)
                .Select(ubr => new BuildingWithRoleViewModel
                {
                    Building = ubr.Building,
                    Role = ubr.Role
                })
                .ToListAsync();

            return View(buildingsWithRoles);
        }
        [Authorize(Policy = "BuildingAccess")]
        public async Task<IActionResult> Details(int buildingId)
        {
            var building = await _buildingService.GetBuildingByIdAsync(buildingId);
            if (building == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == currentUserId);

            if (userProfile == null) return NotFound();

            ViewBag.UserRole = await _buildingService.GetUserRoleInBuildingAsync(userProfile.Id, building.Id);
            ViewBag.Managers = await _buildingService.GetBuildingManagersAsync(building.Id);

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;

            return View(building);
        }
        [Authorize(Policy = "BuildingAccess")]
        public async Task<IActionResult> Dashboard(int buildingId)
        {
            // Bina bilgilerini getir
            var building = await _context.Buildings
                .Include(b => b.Incomes)
                .Include(b => b.Expenses)
                .Include(b => b.Units)
                .Include(b => b.Announcements)
                .Include(b => b.UserDebts)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null)
            {
                return NotFound();
            }

            // Kullanýcý bilgilerini getir
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserDebt = await _context.UserDebts
                .Where(ud => ud.UserId == userId && ud.BuildingId == buildingId)
                .SumAsync(ud => ud.Amount);

            // ViewBag verileri
            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;
            ViewBag.BuildingType = building.Type;
            ViewBag.BuildingAddress = building.Address;
            ViewBag.UnitCount = building.Units.Count;

            // Finansal bilgiler
            ViewBag.TotalIncome = building.Incomes.Sum(i => i.Amount);
            ViewBag.TotalExpense = building.Expenses.Sum(e => e.Amount);
            ViewBag.Balance = ViewBag.TotalIncome - ViewBag.TotalExpense;
            ViewBag.UserDebt = currentUserDebt;

            // Son ödeme tarihi
            var lastPayment = await _context.Incomes
                .Where(i => i.BuildingId == buildingId && i.PayerId==userId)
                .OrderByDescending(i => i.Date)
                .FirstOrDefaultAsync();
            ViewBag.LastPaymentDate = lastPayment?.Date;

            // Son 6 ayýn gelir-gider verileri
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            ViewBag.LastSixMonths = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i).ToString("MMM yyyy"))
                .Reverse()
                .ToList();

            ViewBag.IncomeByMonth = Enumerable.Range(0, 6)
                .Select(i => building.Incomes
                    .Where(inc => inc.Date.Month == DateTime.Now.AddMonths(-i).Month &&
                           inc.Date.Year == DateTime.Now.AddMonths(-i).Year)
                    .Sum(inc => inc.Amount))
                .Reverse()
                .ToList();

            ViewBag.ExpenseByMonth = Enumerable.Range(0, 6)
                .Select(i => building.Expenses
                    .Where(exp => exp.Date.Month == DateTime.Now.AddMonths(-i).Month &&
                           exp.Date.Year == DateTime.Now.AddMonths(-i).Year)
                    .Sum(exp => exp.Amount))
                .Reverse()
                .ToList();

            // Aidat durumu
            var totalUnits = building.Units.Count;
            var paidDues = await _context.UserDebts
                .Where(ud => ud.BuildingId == buildingId && ud.Amount <= 0)
                .CountAsync();
            var unpaidDues = await _context.UserDebts
                .Where(ud => ud.BuildingId == buildingId && ud.Amount > 0)
                .CountAsync();

            ViewBag.PaidDues = paidDues;
            ViewBag.UnpaidDues = unpaidDues;
            ViewBag.PartiallyPaidDues = 0; // Kýsmi ödeme durumunuza göre güncelleyin
            ViewBag.PaidDuesPercentage = totalUnits > 0 ? (int)((paidDues * 100) / totalUnits) : 0;

            // Diðer istatistikler
            ViewBag.PendingComplaints = await _context.Complaints
     .Include(c => c.Unit) // Unit bilgilerini dahil ediyoruz
     .Where(c => c.Unit.BuildingId == buildingId && c.Status == "Beklemede")
     .CountAsync();

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            ViewBag.RecentIncomeCount = building.Incomes.Count(i => i.Date >= thirtyDaysAgo);
            ViewBag.RecentExpenseCount = building.Expenses.Count(e => e.Date >= thirtyDaysAgo);
            ViewBag.RecentAnnouncements = building.Announcements.Count(a => a.CreatedAt >= thirtyDaysAgo);

            return View(building);
        }


        [Authorize(Policy = "BuildingAccess")]
        public async Task<IActionResult> Settings(int buildingId)
        {
            var building = await _buildingService.GetBuildingByIdAsync(buildingId);
            if (building == null) return NotFound();

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;

            return View();
        }

        // GET: Buildings/Create
        [Route("/Buildings/Create")]
        public IActionResult Create()
        {
            return View(new BuildingViewModel());
        }

        // POST: Buildings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Buildings/Create")]
        public async Task<IActionResult> Create(BuildingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _buildingService.IsBuildingExistsAsync(model.Name, model.Address))
            {
                ModelState.AddModelError("", "Bu isim ve adreste zaten bir bina kayýtlý");
                return View(model);
            }

            string imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "BuildsImage");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                imageUrl = $"/BuildsImage/{uniqueFileName}";
            }

            var currentUserId = _userManager.GetUserId(User);
            var building = new Building
            {
                Name = model.Name,
                Address = model.Address,
                Type = model.Type,
                Block = model.Block,
                FloorCount = model.FloorCount,
                UnitCount = model.UnitCount,
                Description = model.Description,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now
            };

            var (success, message) = await _buildingService.CreateBuildingAsync(building, currentUserId);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = message;
            }

            return View(model);
        }

        // GET: Buildings/Edit/5
        [Authorize(Policy = "BuildingAccess")]
        public async Task<IActionResult> Edit(int buildingId)
        {
            var building = await _buildingService.GetBuildingByIdAsync(buildingId);
            if (building == null) return NotFound();

            var model = new BuildingViewModel
            {
                Id = building.Id,
                Name = building.Name,
                Address = building.Address,
                Block = building.Block,
                Type = building.Type,
                FloorCount = building.FloorCount,
                UnitCount = building.UnitCount,
                Description = building.Description,
                CurrentImageUrl = building.ImageUrl
            };

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;

            return View(model);
        }

        // POST: Buildings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "BuildingAccess")]
        public async Task<IActionResult> Edit(int buildingId, BuildingViewModel model)
        {
            if (buildingId != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var building = await _buildingService.GetBuildingByIdAsync(buildingId);
            if (building == null) return NotFound();

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "BuildsImage");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (!string.IsNullOrEmpty(building.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, building.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                building.ImageUrl = $"/BuildsImage/{uniqueFileName}";
            }

            building.Name = model.Name;
            building.Address = model.Address;
            building.Type = model.Type;
            building.Block = model.Block;
            building.FloorCount = model.FloorCount;
            building.UnitCount = model.UnitCount;
            building.Description = model.Description;

            _context.Update(building);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bina bilgileri baþarýyla güncellendi";
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Policy = "BuildingAccess")]
        // GET: Buildings/Delete/5
        [Authorize(Policy = "BuildingAdmin")]
        public async Task<IActionResult> Delete(int buildingId)
        {
            var building = await _buildingService.GetBuildingByIdAsync(buildingId);
            if (building == null) return NotFound();

            ViewBag.BuildingId = building.Id;
            ViewBag.BuildingName = building.Name;

            return View(building);
        }

        // POST: Buildings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "BuildingAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int buildingId)
        {
            var building = await _buildingService.GetBuildingByIdAsync(buildingId);
            if (building == null) return NotFound();

            if (!string.IsNullOrEmpty(building.ImageUrl))
            {
                var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, building.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bina baþarýyla silindi";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> BuildingExists(int id)
        {
            return await _context.Buildings.AnyAsync(e => e.Id == id);
        }
    }
}
