using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;

[Authorize]
public class ExpenseController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExpenseController(ApplicationDbContext context)
    {
        _context = context;
    }

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

    public IActionResult Create(int buildingId)
    {
        ViewBag.BuildingId = buildingId;
        return View(new Expense { BuildingId = buildingId, Date = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Expense expense)
    {
        if (ModelState.IsValid)
        {
            expense.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _context.Add(expense);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { buildingId = expense.BuildingId });
        }
        ViewBag.BuildingId = expense.BuildingId;
        return View(expense);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var expense = await _context.Expenses
            .Include(e => e.Building)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (expense == null) return NotFound();

        ViewBag.BuildingId = expense.BuildingId;
        return View(expense);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Expense expense)
    {
        if (id != expense.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existingExpense = await _context.Expenses.FindAsync(id);
                if (existingExpense == null) return NotFound();

                existingExpense.Amount = expense.Amount;
                existingExpense.Date = expense.Date;
                existingExpense.Type = expense.Type;
                existingExpense.Description = expense.Description;
                existingExpense.PaymentMethod = expense.PaymentMethod;
                existingExpense.ReceiptNumber = expense.ReceiptNumber;
                existingExpense.ReceiptImageUrl = expense.ReceiptImageUrl;
                existingExpense.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { buildingId = existingExpense.BuildingId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Expenses.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
        }
        ViewBag.BuildingId = expense.BuildingId;
        return View(expense);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var expense = await _context.Expenses
            .Include(e => e.Building)
            .Include(e => e.RecordedBy)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (expense == null) return NotFound();

        return View(expense);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        var buildingId = expense.BuildingId;

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { buildingId });
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var expense = await _context.Expenses
            .Include(e => e.Building)
            .Include(e => e.RecordedBy)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (expense == null) return NotFound();

        return View(expense);
    }
}
