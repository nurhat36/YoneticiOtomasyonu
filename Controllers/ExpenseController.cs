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
        var expenseTypes = new List<SelectListItem>
    {
        new SelectListItem { Value = "Temizlik", Text = "Temizlik" },
        new SelectListItem { Value = "Bakım", Text = "Bakım" },
        new SelectListItem { Value = "Elektrik", Text = "Elektrik" },
        new SelectListItem { Value = "Su", Text = "Su" },
        new SelectListItem { Value = "Diğer", Text = "Diğer" }
    };

        var paymentMethods = new List<SelectListItem>
    {
        new SelectListItem { Value = "Nakit", Text = "Nakit" },
        new SelectListItem { Value = "Kredi Kartı", Text = "Kredi Kartı" },
        new SelectListItem { Value = "Havale", Text = "Havale" },
        new SelectListItem { Value = "Diğer", Text = "Diğer" }
    };

        ViewBag.ExpenseTypes = expenseTypes;
        ViewBag.PaymentMethods = paymentMethods;

        var expense = new Expense
        {
            BuildingId = buildingId,
            Date = DateTime.Now
        };

        return View(expense);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Expense expense, IFormFile receiptFile)
    {
        if (!ModelState.IsValid)
        {
            // Kaydeden kullanıcıyı ata
            expense.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Eğer dosya yüklenmişse kaydet
            if (receiptFile != null && receiptFile.Length > 0)
            {
                // Dosya yolu (ör: wwwroot/uploads/)
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Document");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(receiptFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await receiptFile.CopyToAsync(fileStream);
                }

                // Veritabanına kaydederken sadece dosya adını veya yolunu kaydediyoruz
                expense.ReceiptImageUrl = "/Document/" + uniqueFileName;
            }

            _context.Add(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { buildingId = expense.BuildingId });
        }

        // ViewBag doldur
        var expenseTypes = new List<SelectListItem>
    {
        new SelectListItem { Value = "Temizlik", Text = "Temizlik" },
        new SelectListItem { Value = "Bakım", Text = "Bakım" },
        new SelectListItem { Value = "Elektrik", Text = "Elektrik" },
        new SelectListItem { Value = "Su", Text = "Su" },
        new SelectListItem { Value = "Diğer", Text = "Diğer" }
    };

        var paymentMethods = new List<SelectListItem>
    {
        new SelectListItem { Value = "Nakit", Text = "Nakit" },
        new SelectListItem { Value = "Kredi Kartı", Text = "Kredi Kartı" },
        new SelectListItem { Value = "Havale", Text = "Havale" },
        new SelectListItem { Value = "Diğer", Text = "Diğer" }
    };

        ViewBag.ExpenseTypes = expenseTypes;
        ViewBag.PaymentMethods = paymentMethods;

        return View(expense);
    }


    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var expense = await _context.Expenses
            .Include(e => e.Building)
            .Include(e => e.RecordedBy)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null) return NotFound();

        // Gider türleri
        ViewBag.ExpenseTypes = new List<string> { "Temizlik", "Bakım", "Elektrik", "Su", "Diğer" };

        // Ödeme yöntemleri
        ViewBag.PaymentMethods = new List<string> { "Nakit", "Kredi Kartı", "Havale", "Çek", "Diğer" };

        return View(expense);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Date,Type,Description,PaymentMethod,ReceiptNumber,BuildingId")] Expense expense, IFormFile NewReceiptFile)
    {
        if (id != expense.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            try
            {
                var existingExpense = await _context.Expenses.FindAsync(id);
                if (existingExpense == null) return NotFound();

                // Güncellenebilir alanlar
                existingExpense.Amount = expense.Amount;
                existingExpense.Date = expense.Date;
                existingExpense.Type = expense.Type;
                existingExpense.Description = expense.Description;
                existingExpense.PaymentMethod = expense.PaymentMethod;
                existingExpense.ReceiptNumber = expense.ReceiptNumber;

                // Dosya yüklenmişse işleme al
                if (NewReceiptFile != null && NewReceiptFile.Length > 0)
                {
                    // Benzersiz isim oluştur
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(NewReceiptFile.FileName);

                    // wwwroot/uploads klasörüne kaydet
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await NewReceiptFile.CopyToAsync(stream);
                    }

                    // URL'yi kaydet
                    existingExpense.ReceiptImageUrl = "/uploads/" + fileName;
                }

                // Kaydeden kullanıcıyı güncelle (edit yapan kişi)
                existingExpense.RecordedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Expenses.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("Index", new { buildingId = expense.BuildingId });
        }

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
