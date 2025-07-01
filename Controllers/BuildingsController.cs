using System;
using System.IO;
using System.Linq;
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

        // GET: Buildings
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == currentUserId);

            if (userProfile == null)
            {
                return NotFound();
            }

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

        // GET: Buildings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var building = await _buildingService.GetBuildingByIdAsync(id.Value);
            if (building == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == currentUserId);

            if (userProfile == null) return NotFound();

            ViewBag.UserRole = await _buildingService.GetUserRoleInBuildingAsync(userProfile.Id, building.Id);
            return View(building);
        }

        // GET: Buildings/Create
        public IActionResult Create()
        {
            return View(new BuildingViewModel());
        }

        // POST: Buildings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BuildingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Check if building with same name and address already exists
                if (await _buildingService.IsBuildingExistsAsync(model.Name, model.Address))
                {
                    ModelState.AddModelError("", "Bu isim ve adreste zaten bir bina kayýtlý");
                    return View(model);
                }

                // Handle image upload
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
            }

            return View(model);
        }

        // GET: Buildings/Edit/5
        [Authorize(Policy = "BuildingAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var building = await _buildingService.GetBuildingByIdAsync(id.Value);
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

            return View(model);
        }

        // POST: Buildings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "BuildingAdmin")]
        public async Task<IActionResult> Edit(int id, BuildingViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    var building = await _buildingService.GetBuildingByIdAsync(id);
                    if (building == null) return NotFound();

                    // Handle image upload
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "BuildsImage");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Delete old image if exists
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

                    // Update other properties
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
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BuildingExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }

        // GET: Buildings/Delete/5
        [Authorize(Policy = "BuildingAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var building = await _buildingService.GetBuildingByIdAsync(id.Value);
            if (building == null) return NotFound();

            return View(building);
        }

        // POST: Buildings/Delete/5 rfgbdabdfb
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "BuildingAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var building = await _buildingService.GetBuildingByIdAsync(id);
            if (building == null) return NotFound();

            try
            {
                // Delete image file if exists
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
            catch (Exception ex)
            {
                TempData["Error"] = $"Bina silinirken hata oluþtu: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }


        private async Task<bool> BuildingExists(int id)
        {
            return await _context.Buildings.AnyAsync(e => e.Id == id);
        }
    }
}