using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;

[Authorize]
public class ComplaintController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ComplaintController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Listeleme
    public async Task<IActionResult> Index()
    {
        var complaints = await _context.Complaints
            .Include(c => c.Unit)
            .Include(c => c.Complainant)
            .Include(c => c.AssignedTo)
            .Include(c => c.Images)  // Resimler burada yüklenmeli
            .ToListAsync();

        return View(complaints);
    }

    // Yeni şikayet formu
    public IActionResult Create()
    {
        ViewBag.Units = new SelectList(_context.Units, "Id", "Number");
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Complaint complaint, List<IFormFile> ImageFiles)
    {
        var userId = _userManager.GetUserId(User);
        complaint.ComplainantId = userId;
        complaint.CreatedAt = DateTime.Now;
        complaint.Status = "Beklemede";

        _context.Add(complaint);
        await _context.SaveChangesAsync(); // Önce complaint kaydet ki ID oluşsun

        if (ImageFiles != null && ImageFiles.Count > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "complaints");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var file in ImageFiles)
            {
                if (file.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var complaintImage = new ComplaintImage
                    {
                        ComplaintId = complaint.Id,
                        ImageUrl = $"/complaints/{fileName}"
                    };
                    _context.ComplaintImages.Add(complaintImage);
                }
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }


    // Detay sayfası
    public async Task<IActionResult> Details(int id)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Unit)
            .Include(c => c.Complainant)
            .Include(c => c.AssignedTo)
            .Include(c => c.Images)  // Resimler burada yüklenmeli
            .FirstOrDefaultAsync(c => c.Id == id);

        if (complaint == null)
        {
            return NotFound();
        }

        var statusList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Beklemede", Value = "Beklemede" },
        new SelectListItem { Text = "Atandı", Value = "Atandı" },
        new SelectListItem { Text = "Çözüldü", Value = "Çözüldü" },
        new SelectListItem { Text = "Reddedildi", Value = "Reddedildi" }
    };

        // Seçili olanı işaretle
        foreach (var s in statusList)
        {
            if (s.Value == complaint.Status)
                s.Selected = true;
        }

        ViewBag.StatusList = statusList;
        var userList = await _context.Users
    .Select(u => new SelectListItem
    {
        Value = u.Id,
        Text = u.UserName,
        Selected = (u.Id == complaint.AssignedToId)
    }).ToListAsync();

        ViewBag.UserList = userList;

        // Kullanıcı listesi
        var users = _context.Users.ToList();
        ViewBag.Users = users;

        return View(complaint);
    }


    // Yönetici cevabı
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(int id, string response, string status, string assignedToId)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null) return NotFound();

        complaint.Response = response;
        complaint.Status = status;
        complaint.ResponseDate = DateTime.Now;
        complaint.AssignedToId = string.IsNullOrEmpty(assignedToId) ? null : assignedToId;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
