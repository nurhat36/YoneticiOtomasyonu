using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models.ViewModels;

namespace YoneticiOtomasyonu.Controllers
{
    [Authorize]
    public class UnitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnitsController(ApplicationDbContext context)
        {
            _context = context;
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

            TempData["Success"] = "Birim başarıyla eklendi";
            return RedirectToAction("Details", "Buildings", new { id = model.BuildingId });
        }
        public async Task<IActionResult> List(int buildingId)
        {
            var building = await _context.Buildings
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null) return NotFound();

            // Her unit’in resident bilgisi de gelsin
            var units = await _context.Units
                .Include(u => u.Resident)
                .Where(u => u.BuildingId == buildingId)
                .ToListAsync();

            ViewBag.BuildingName = building.Name;
            ViewBag.BuildingId = buildingId;

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

            // Kat listesi oluştur
            var floors = Enumerable.Range(0, building.FloorCount + 1)
                .Select(f => new SelectListItem
                {
                    Value = f.ToString(),
                    Text = f == 0 ? "Zemin" : $"{f}. Kat"
                }).ToList();

            var users = await _context.Users.ToListAsync();

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
                FloorList = floors
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
            unit.ResidentId = model.ResidentId;

            _context.Update(unit);
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
