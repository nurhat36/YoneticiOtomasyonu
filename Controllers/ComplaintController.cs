using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
namespace YoneticiOtomasyonu.Controllers
{
    [Authorize(Policy = "BuildingAccess")]
    [Route("Buildings/{buildingId:int}/[controller]")]
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
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int buildingId)
        {
            var complaints = await _context.Complaints
                .Include(c => c.Unit)
                .Include(c => c.Complainant)
                .Include(c => c.AssignedTo)
                .Include(c => c.Images)  // Resimler burada yüklenmeli
                .ToListAsync();
            ViewBag.BuildingId = buildingId; // Binaya göre filtreleme için BuildingId'yi ViewBag'e ekle
            return View(complaints);
        }
        // Yeni şikayet formu
        [HttpGet("Create")]
        public IActionResult Create(int buildingId)
        {
            ViewBag.Units = new SelectList(_context.Units.Where(u => u.BuildingId == buildingId), "Id", "Number");
            ViewBag.BuildingId = buildingId;
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int buildingId, Complaint complaint, List<IFormFile> ImageFiles)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Units = new SelectList(_context.Units.Where(u => u.BuildingId == buildingId), "Id", "Number");
                ViewBag.BuildingId = buildingId;
                return View(complaint);
            }

            var userId = _userManager.GetUserId(User);
            complaint.ComplainantId = userId;
            complaint.CreatedAt = DateTime.Now;
            complaint.Status = "Beklemede";
            complaint.UnitId = complaint.UnitId; // Ensure this is set from form

            _context.Add(complaint);
            await _context.SaveChangesAsync(); // ID oluşturmak için önce kaydet

            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "complaints");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
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

            return RedirectToAction(nameof(Index), new { buildingId });
        }


        // Detay sayfası
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int buildingId, int id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Unit)
                .Include(c => c.Complainant)
                .Include(c => c.AssignedTo)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
                return NotFound();

            var statusList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Beklemede", Value = "Beklemede" },
        new SelectListItem { Text = "Atandı", Value = "Atandı" },
        new SelectListItem { Text = "Çözüldü", Value = "Çözüldü" },
        new SelectListItem { Text = "Reddedildi", Value = "Reddedildi" }
    };

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

            ViewBag.Users = userList; // Burada "Users" olarak atandı
            ViewBag.BuildingId = buildingId;

            return View(complaint);
        }


        // Yönetici cevabı
        [HttpPost("/building/{buildingId}/Complaint/{id}/Respond")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int buildingId, int id, string response, string status, string assignedToId, string priority)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Unit)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
                return NotFound();

            complaint.Response = response;
            complaint.Status = status;
            complaint.ResponseDate = DateTime.Now;
            complaint.AssignedToId = string.IsNullOrEmpty(assignedToId) ? null : assignedToId;

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id;

            var buildingIdFromComplaint = complaint.Unit?.BuildingId ?? 0;

            if (!string.IsNullOrEmpty(assignedToId))
            {
                var workTask = new WorkTask
                {
                    Title = $"Şikayet #{complaint.Id} Görevi",
                    Description = $"Bu görev şikayet #{complaint.Id} yanıtlanmasına istinaden oluşturulmuştur.",
                    CreatedAt = DateTime.Now,
                    Status = "Beklemede",
                    Priority = priority,
                    ComplaintId = complaint.Id,
                    BuildingId = buildingIdFromComplaint,
                    CreatedById = currentUserId,
                    AssignedToId = assignedToId
                };

                _context.WorkTasks.Add(workTask);

                string message = $"Sayın kullanıcı, sizi {buildingIdFromComplaint} numaralı binada bir göreve eklendiniz. Görevin önceliği {priority}. Lütfen en kısa zamanda ilgileniniz.";
                string link = Url.Action("MyTasks", "WorkTask");

                await AddNotification(assignedToId, message, link);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { buildingId });
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
    }
}
