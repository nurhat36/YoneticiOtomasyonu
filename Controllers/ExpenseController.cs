using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
namespace YoneticiOtomasyonu.Controllers { 
[Authorize(Policy = "BuildingAccess")]
[Route("Buildings/{buildingId:int}/Expense")]
public class ExpenseController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExpenseController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int buildingId)
    {
        var expenses = await _context.Expenses
            .Include(e => e.Building)
            .Include(e => e.RecordedBy)
            .Where(e => e.BuildingId == buildingId)
            .ToListAsync();

        ViewBag.BuildingId = buildingId;
        return View(expenses);
    }

    [HttpGet("Create")]
    public IActionResult Create(int buildingId)
    {
        ViewBag.ExpenseTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "Temizlik", Text = "Temizlik" },
            new SelectListItem { Value = "Bakım", Text = "Bakım" },
            new SelectListItem { Value = "Elektrik", Text = "Elektrik" },
            new SelectListItem { Value = "Su", Text = "Su" },
            new SelectListItem { Value = "Diğer", Text = "Diğer" }
        };

        ViewBag.PaymentMethods = new List<SelectListItem>
        {
            new SelectListItem { Value = "Nakit", Text = "Nakit" },
            new SelectListItem { Value = "Kredi Kartı", Text = "Kredi Kartı" },
            new SelectListItem { Value = "Havale", Text = "Havale" },
            new SelectListItem { Value = "Diğer", Text = "Diğer" }
        };
        ViewBag.BuildingId = buildingId;

        var expense = new Expense
        {
            BuildingId = buildingId,
            Date = DateTime.Now
        };

        return View(expense);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int buildingId, Expense expense, IFormFile receiptFile)
    {
        if (!ModelState.IsValid)
        {
            expense.BuildingId = buildingId;
            expense.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (receiptFile != null && receiptFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Document");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(receiptFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await receiptFile.CopyToAsync(stream);
                }

                expense.ReceiptImageUrl = "/Document/" + uniqueFileName;
            }

            _context.Add(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { buildingId });
        }

        ViewBag.ExpenseTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "Temizlik", Text = "Temizlik" },
            new SelectListItem { Value = "Bakım", Text = "Bakım" },
            new SelectListItem { Value = "Elektrik", Text = "Elektrik" },
            new SelectListItem { Value = "Su", Text = "Su" },
            new SelectListItem { Value = "Diğer", Text = "Diğer" }
        };

        ViewBag.PaymentMethods = new List<SelectListItem>
        {
            new SelectListItem { Value = "Nakit", Text = "Nakit" },
            new SelectListItem { Value = "Kredi Kartı", Text = "Kredi Kartı" },
            new SelectListItem { Value = "Havale", Text = "Havale" },
            new SelectListItem { Value = "Diğer", Text = "Diğer" }
        };

        return View(expense);
    }

    [HttpGet("{id}/Edit")]
    public async Task<IActionResult> Edit(int buildingId, int id)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.BuildingId == buildingId);

        if (expense == null)
            return NotFound();

        ViewBag.ExpenseTypes = new List<string> { "Temizlik", "Bakım", "Elektrik", "Su", "Diğer" };
        ViewBag.PaymentMethods = new List<string> { "Nakit", "Kredi Kartı", "Havale", "Çek", "Diğer" };
        ViewBag.BuildingId = buildingId;

        return View(expense);
    }

    [HttpPost("{id}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int buildingId, int id, [Bind("Id,Amount,Date,Type,Description,PaymentMethod,ReceiptNumber,BuildingId")] Expense expense, IFormFile NewReceiptFile)
    {
        if (id != expense.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            var existingExpense = await _context.Expenses.FindAsync(id);
            if (existingExpense == null)
                return NotFound();

            existingExpense.Amount = expense.Amount;
            existingExpense.Date = expense.Date;
            existingExpense.Type = expense.Type;
            existingExpense.Description = expense.Description;
            existingExpense.PaymentMethod = expense.PaymentMethod;
            existingExpense.ReceiptNumber = expense.ReceiptNumber;

            if (NewReceiptFile != null && NewReceiptFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(NewReceiptFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await NewReceiptFile.CopyToAsync(stream);
                }

                existingExpense.ReceiptImageUrl = "/uploads/" + fileName;
            }

            existingExpense.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { buildingId });
        }

        return View(expense);
    }

    [HttpGet("{id}/Delete")]
    public async Task<IActionResult> Delete(int buildingId, int id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Building)
            .Include(e => e.RecordedBy)
            .FirstOrDefaultAsync(e => e.Id == id && e.BuildingId == buildingId);

        if (expense == null)
            return NotFound();
        ViewBag.BuildingId = buildingId;

        return View(expense);
    }

    [HttpPost("{id}/Delete"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int buildingId, int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
            return NotFound();

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { buildingId });
    }

    [HttpGet("{id}/Details")]
    public async Task<IActionResult> Details(int buildingId, int id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Building)
            .Include(e => e.RecordedBy)
            .FirstOrDefaultAsync(e => e.Id == id && e.BuildingId == buildingId);

        if (expense == null)
            return NotFound();
        ViewBag.BuildingId = buildingId;

        return View(expense);
    }
}
}