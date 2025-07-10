using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models.ViewModels;

namespace YoneticiOtomasyonu.Controllers
{
    [Route("Buildings/{buildingId:int}/[controller]/[action]")]
    [Authorize]
    public class UnitsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UnitsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Units/Add?buildingId=5
        public async Task<IActionResult> Add(int buildingId)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            if (building == null) return NotFound();

            var users = await _context.Users.ToListAsync();

            // 🔥 Buraya ekleyeceğiz!
            var floors = Enumerable.Range(0, building.FloorCount + 1)
                .Select(f => new SelectListItem
                {
                    Value = f.ToString(),
                    Text = f == 0 ? "Zemin" : $"{f}. Kat"
                }).ToList();

            var model = new UnitViewModel
            {
                BuildingId = buildingId,
                Residents = users,
                FloorList = floors
            };

            return View(model);
        }



        // POST: Units/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(UnitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Kullanıcı listesi tekrar doldurulmalı (view tekrar açılınca boş olmasın)
                model.Residents = await _context.Users.ToListAsync();
                return View(model);
            }

            var unit = new Unit
            {
                Number = model.Number,
                Type = model.Type,
                Floor = model.Floor,
                Area = model.Area,
                IsOccupied = model.IsOccupied,
                Description = model.Description,
                BuildingId = model.BuildingId,
                ResidentId = model.IsOccupied ? model.ResidentId : null
            };

            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            if (model.IsOccupied && !string.IsNullOrEmpty(model.ResidentId))
            {
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.IdentityUserId == model.ResidentId);
                if (userProfile != null && !string.IsNullOrEmpty(model.SelectedRole))
                {
                    var newRole = new UserBuildingRole
                    {
                        UserProfileId = userProfile.Id,
                        BuildingId = model.BuildingId,
                        Role = model.SelectedRole,
                        IsPrimary = false,
                        AssignmentDate = DateTime.Now,
                        AssignedByUserId = _userManager.GetUserId(User)
                    };

                    _context.UserBuildingRoles.Add(newRole);
                    await _context.SaveChangesAsync();
                    string message = $"Sayın kullanıcı, sizi {model.BuildingId} numaralı bina  daire {model.Number} ekledik.";
                    string link = Url.Action("Details", "Buildings", new { id = model.BuildingId });

                    await AddNotification(model.ResidentId, message, link);
                }
            }


            TempData["Success"] = "Birim başarıyla eklendi";
            return RedirectToAction("Details", "Buildings", new { id = model.BuildingId });
        }
        private async Task AddNotification(string userId, string message, string? link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Link = link,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> List(int buildingId, string status = "all", string unitType = "all")
        {
            var building = await _context.Buildings
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null) return NotFound();

            // Unit'leri Resident ile al ve filtre uygula
            var unitsQuery = _context.Units
                .Include(u => u.Resident)
                .Where(u => u.BuildingId == buildingId);

            // 🔹 Dolu/boş filtreleme
            if (status == "occupied")
                unitsQuery = unitsQuery.Where(u => u.Resident != null);
            else if (status == "empty")
                unitsQuery = unitsQuery.Where(u => u.Resident == null);

            // 🔹 Birim türü filtreleme (örneğin: Daire, Ofis, Depo)
            if (unitType != "all")
                unitsQuery = unitsQuery.Where(u => u.Type.ToLower() == unitType.ToLower());

            var units = await unitsQuery.ToListAsync();

            // Kullanıcının rol bilgisi ekleniyor
            foreach (var unit in units.Where(u => u.Resident != null))
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.IdentityUserId == unit.Resident.Id);

                if (userProfile != null)
                {
                    var role = await _context.UserBuildingRoles
                        .Where(r => r.UserProfileId == userProfile.Id && r.BuildingId == buildingId)
                        .Select(r => r.Role)
                        .FirstOrDefaultAsync();

                    unit.Description += $" [Rol: {role}]";
                }
            }

            ViewBag.BuildingName = building.Name;
            ViewBag.BuildingId = buildingId;
            ViewBag.Status = status;
            ViewBag.UnitType = unitType;

            return View(units);
        }


        public async Task<IActionResult> Details(int id)
        {
            var unit = await _context.Units
                .Include(u => u.Building)
                .Include(u => u.Resident)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null) return NotFound();

            return View(unit);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _context.Units
                .Include(u => u.Building)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null) return NotFound();

            var building = await _context.Buildings.FindAsync(unit.BuildingId);

            // Kat listesi
            var floors = Enumerable.Range(0, building.FloorCount + 1)
                .Select(f => new SelectListItem
                {
                    Value = f.ToString(),
                    Text = f == 0 ? "Zemin" : $"{f}. Kat"
                }).ToList();

            var users = await _context.Users.ToListAsync();

            // Seçilen kullanıcının mevcut rolünü bulalım
            var userRole = await _context.UserBuildingRoles
                .Where(r => r.BuildingId == unit.BuildingId && r.UserProfile.IdentityUserId == unit.ResidentId)
                .Select(r => r.Role)
                .FirstOrDefaultAsync();

            var model = new UnitViewModel
            {
                Id = unit.Id,
                Number = unit.Number,
                Type = unit.Type,
                Floor = unit.Floor,
                Area = unit.Area,
                IsOccupied = unit.IsOccupied,
                Description = unit.Description,
                BuildingId = unit.BuildingId,
                ResidentId = unit.ResidentId,
                Residents = users,
                FloorList = floors,
                SelectedRole = userRole // 👈 Mevcut rolü ekledik
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UnitViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                // Kat listesi ve kullanıcıları tekrar doldur
                var building = await _context.Buildings.FindAsync(model.BuildingId);
                model.FloorList = Enumerable.Range(0, building.FloorCount + 1)
                    .Select(f => new SelectListItem
                    {
                        Value = f.ToString(),
                        Text = f == 0 ? "Zemin" : $"{f}. Kat"
                    }).ToList();
                model.Residents = await _context.Users.ToListAsync();
                return View(model);
            }

            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();

            unit.Number = model.Number;
            unit.Type = model.Type;
            unit.Floor = model.Floor;
            unit.Area = model.Area;
            unit.IsOccupied = model.IsOccupied;
            unit.Description = model.Description;
            unit.ResidentId = model.IsOccupied ? model.ResidentId : null;

            _context.Update(unit);

            // Kullanıcı-rol tablosunu güncelle
            if (model.IsOccupied && !string.IsNullOrEmpty(model.ResidentId) && !string.IsNullOrEmpty(model.SelectedRole))
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.IdentityUserId == model.ResidentId);

                if (userProfile != null)
                {
                    var existingRole = await _context.UserBuildingRoles
                        .FirstOrDefaultAsync(r => r.BuildingId == model.BuildingId && r.UserProfileId == userProfile.Id);

                    if (existingRole != null)
                    {
                        existingRole.Role = model.SelectedRole;
                        existingRole.AssignmentDate = DateTime.Now;
                        _context.UserBuildingRoles.Update(existingRole);
                    }
                    else
                    {
                        var newRole = new UserBuildingRole
                        {
                            UserProfileId = userProfile.Id,
                            BuildingId = model.BuildingId,
                            Role = model.SelectedRole,
                            IsPrimary = false,
                            AssignmentDate = DateTime.Now,
                            AssignedByUserId = _userManager.GetUserId(User)
                        };
                        _context.UserBuildingRoles.Add(newRole);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Birim başarıyla güncellendi.";
            return RedirectToAction("List", new { buildingId = model.BuildingId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _context.Units
                .Include(u => u.Building)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null) return NotFound();

            return View(unit);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return NotFound();

            var buildingId = unit.BuildingId;

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Birim başarıyla silindi.";
            return RedirectToAction("List", new { buildingId });
        }


    }
}
