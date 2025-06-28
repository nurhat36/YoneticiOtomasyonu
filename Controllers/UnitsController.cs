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

    }
}
