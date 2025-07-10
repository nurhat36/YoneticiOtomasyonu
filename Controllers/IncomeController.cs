using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;

namespace YoneticiOtomasyonu.Controllers
{
    [Route("Buildings/{buildingId}/Income")]
    public class IncomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listeleme
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int buildingId)
        {
            var incomes = await _context.Incomes
                .Include(i => i.Building)
                .Include(i => i.Unit)
                .Include(i => i.Payer)
                .Include(i => i.RecordedBy)
                .Where(i => i.BuildingId == buildingId)
                .ToListAsync();

            ViewBag.BuildingId = buildingId;
            return View(incomes);
        }

        // Detaylar
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int buildingId, int id)
        {
            var income = await _context.Incomes
                .Include(i => i.Building)
                .Include(i => i.Unit)
                .Include(i => i.Payer)
                .Include(i => i.RecordedBy)
                .FirstOrDefaultAsync(m => m.Id == id && m.BuildingId == buildingId);

            if (income == null) return NotFound();

            ViewBag.BuildingId = buildingId;
            return View(income);
        }

        // Ekleme GET
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int buildingId)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            if (building == null) return NotFound();

            var units = await _context.Units.Where(u => u.BuildingId == buildingId).ToListAsync();
            ViewBag.UnitId = new SelectList(units, "Id", "Number");
            ViewBag.BuildingId = buildingId;

            return View();
        }

        // Ekleme POST
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int buildingId, [Bind("Amount,Date,Type,Description,PaymentMethod,UnitId")] Income income)
        {
            if (ModelState.IsValid)
            {
                income.BuildingId = buildingId;
                income.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (income.UnitId.HasValue)
                {
                    var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == income.UnitId.Value);
                    if (unit != null)
                    {
                        income.PayerId = unit.ResidentId;
                    }
                }

                _context.Add(income);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { buildingId });
            }

            var units = await _context.Units.Where(u => u.BuildingId == buildingId).ToListAsync();
            ViewBag.UnitId = new SelectList(units, "Id", "Number", income.UnitId);
            ViewBag.BuildingId = buildingId;

            return View(income);
        }

        // Düzenleme GET
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int buildingId, int id)
        {
            var income = await _context.Incomes
                .Include(i => i.Unit)
                .ThenInclude(u => u.Resident)
                .FirstOrDefaultAsync(i => i.Id == id && i.BuildingId == buildingId);

            if (income == null) return NotFound();

            var units = await _context.Units.Where(u => u.BuildingId == buildingId).ToListAsync();
            ViewBag.UnitId = new SelectList(units, "Id", "Number", income.UnitId);
            ViewBag.BuildingId = buildingId;

            return View(income);
        }

        // Düzenleme POST
        [HttpPost("{id}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int buildingId, int id, [Bind("Id,Amount,Date,Type,Description,PaymentMethod,UnitId")] Income income)
        {
            if (id != income.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingIncome = await _context.Incomes.FindAsync(id);
                if (existingIncome == null) return NotFound();

                existingIncome.Amount = income.Amount;
                existingIncome.Date = income.Date;
                existingIncome.Type = income.Type;
                existingIncome.Description = income.Description;
                existingIncome.PaymentMethod = income.PaymentMethod;
                existingIncome.UnitId = income.UnitId;

                if (income.UnitId.HasValue)
                {
                    var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == income.UnitId.Value);
                    if (unit != null)
                    {
                        existingIncome.PayerId = unit.ResidentId;
                    }
                }
                else
                {
                    existingIncome.PayerId = null;
                }

                existingIncome.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { buildingId });
            }

            var units = await _context.Units.Where(u => u.BuildingId == buildingId).ToListAsync();
            ViewBag.UnitId = new SelectList(units, "Id", "Number", income.UnitId);
            ViewBag.BuildingId = buildingId;

            return View(income);
        }

        // Silme GET (Onay ekranı)
        [HttpGet("{id}/Delete")]
        public async Task<IActionResult> Delete(int buildingId, int id)
        {
            var income = await _context.Incomes
                .Include(i => i.Building)
                .Include(i => i.Unit)
                .FirstOrDefaultAsync(m => m.Id == id && m.BuildingId == buildingId);

            if (income == null) return NotFound();

            ViewBag.BuildingId = buildingId;
            return View(income);
        }

        // Silme POST
        [HttpPost("{id}/Delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int buildingId, int id)
        {
            var income = await _context.Incomes.FindAsync(id);
            if (income != null)
            {
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { buildingId });
        }

        // AJAX — Birim bilgisi
        [HttpGet("GetResidentInfo")]
        public IActionResult GetResidentInfo(int unitId)
        {
            var unit = _context.Units.Include(u => u.Resident).FirstOrDefault(u => u.Id == unitId);

            if (unit?.Resident != null)
            {
                return Json(new
                {
                    name = unit.Resident.UserName,
                    profileImage = unit.Resident.ProfileImageUrl
                });
            }

            return Json(null);
        }
    }
}
