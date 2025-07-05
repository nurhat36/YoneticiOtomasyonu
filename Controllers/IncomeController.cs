using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;

namespace YoneticiOtomasyonu.Controllers
{
    public class IncomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listeleme
        public async Task<IActionResult> Index(int buildingId)
        {
            var incomes = _context.Incomes
                .Include(i => i.Building)
                .Include(i => i.Unit)
                .Include(i => i.Payer)
                .Include(i => i.RecordedBy)
                .Where(i => i.BuildingId == buildingId)
                .ToList();

            ViewBag.BuildingId = buildingId; // 👈 Bunu ekle

            return View(incomes);
        }

        // GET: Income/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var income = await _context.Incomes
                .Include(i => i.Building)
                .Include(i => i.Unit)
                .Include(i => i.Payer)
                .Include(i => i.RecordedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (income == null) return NotFound();

            return View(income);
        }
        // GET: Income/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var income = await _context.Incomes
                .Include(i => i.Unit)
                .ThenInclude(u => u.Resident)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (income == null) return NotFound();

            var units = await _context.Units
                .Where(u => u.BuildingId == income.BuildingId)
                .ToListAsync();

            ViewBag.UnitId = new SelectList(units, "Id", "Number", income.UnitId);
            ViewBag.BuildingId = income.BuildingId;

            return View(income);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Date,Type,Description,PaymentMethod,UnitId")] Income income)
        {
            if (id != income.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                try
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

                    return RedirectToAction("Index", new { buildingId = existingIncome.BuildingId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Incomes.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // ModelState invalid ise dropdown'u tekrar yükle
            var units = await _context.Units
                .Where(u => u.BuildingId == income.BuildingId)
                .ToListAsync();

            ViewBag.UnitId = new SelectList(units, "Id", "Number", income.UnitId);
            ViewBag.BuildingId = income.BuildingId;

            return View(income);
        }





        // Ekleme formu GET
        // GET: Income/Create
        public IActionResult Create(int buildingId)
        {
            var building = _context.Buildings.FirstOrDefault(b => b.Id == buildingId);
            if (building == null)
            {
                return NotFound();
            }

            ViewBag.BuildingId = buildingId;

            // O binaya ait birimleri listele
            var units = _context.Units
                .Where(u => u.BuildingId == buildingId)
                .ToList();

            ViewBag.UnitId = new SelectList(units, "Id", "Number"); // "Number" = Birim numarası

            return View();
        }


        // POST: Income/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Amount,Date,Type,Description,PaymentMethod,UnitId")] Income income, int buildingId)
        {
            if (!ModelState.IsValid)
            {
                income.BuildingId = buildingId;
                income.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Eğer Unit seçilmişse, o birimdeki kullanıcı ödeyen olarak atanır
                if (income.UnitId.HasValue)
                {
                    var unit = _context.Units.FirstOrDefault(u => u.Id == income.UnitId.Value);
                    if (unit != null)
                    {
                        income.PayerId = unit.ResidentId;
                    }
                }

                _context.Add(income);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { buildingId = buildingId });
            }

            // Hata durumunda tekrar birim listesini gönder
            var units = _context.Units
                .Where(u => u.BuildingId == buildingId)
                .ToList();
            ViewBag.UnitId = new SelectList(units, "Id", "Number", income.UnitId);
            ViewBag.BuildingId = buildingId;

            return View(income);
        }
        [HttpGet]
        public IActionResult GetResidentInfo(int unitId)
        {
            var unit = _context.Units.Include(u => u.Resident).FirstOrDefault(u => u.Id == unitId);

            if (unit?.Resident != null)
            {
                return Json(new
                {
                    name = unit.Resident.UserName, // veya FullName
                    profileImage = unit.Resident.ProfileImageUrl // Profil resim url'si varsa
                });
            }

            return Json(null);
        }




        // Silme GET (Onay ekranı)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var income = await _context.Incomes
                .Include(i => i.Building)
                .Include(i => i.Unit)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (income == null) return NotFound();

            return View(income);
        }




        // Silme POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var income = await _context.Incomes.FindAsync(id);
            if (income != null)
            {
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
