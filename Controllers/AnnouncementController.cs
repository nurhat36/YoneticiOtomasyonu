using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;

namespace YoneticiOtomasyonu.Controllers
{
    [Authorize(Policy = "BuildingAccess")]
    [Route("Buildings/{buildingId:int}/[controller]")]
    public class AnnouncementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public AnnouncementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // 📄 Liste
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int? buildingId)
        {
            var query = _context.Announcements
                .Include(a => a.Building)
                .Include(a => a.Images)
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();

            if (buildingId.HasValue)
            {
                query = query.Where(a => a.BuildingId == buildingId.Value);
                ViewBag.BuildingId = buildingId.Value; // Eğer view'de kullanmak istersen
            }

            var announcements = await query.ToListAsync();
            return View(announcements);
        }


        // 📄 Detay
        [HttpGet("{id:int}/Detail")]
        public async Task<IActionResult> Detail(int buildingId, int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Images)
                .Include(a => a.Building)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
                return NotFound();
            ViewBag.BuildingId = buildingId; // Binaya göre filtreleme için BuildingId'yi ViewBag'e ekle
            return View(announcement);
        }

        // 📄 Yeni duyuru (GET)
        [HttpGet("Create")]
        public IActionResult Create(int buildingId)
        {
            var announcement = new Announcement
            {
                BuildingId = buildingId
            };
            ViewBag.BuildingId = buildingId; // Binaya göre filtreleme için BuildingId'yi ViewBag'e ekle
            return View(announcement);
        }

        // 📄 Yeni duyuru (POST)
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int buildingId,Announcement announcement, List<IFormFile> images)
        {
            if (ModelState.IsValid)
                return View(announcement);

            announcement.CreatedAt = DateTime.Now;

            var user = await _userManager.GetUserAsync(User);
            announcement.AuthorId = user.Id;

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            // 📷 Resim yükleme
            if (images != null && images.Any())
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "announcements");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var image in images)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var announcementImage = new AnnouncementImage
                    {
                        AnnouncementId = announcement.Id,
                        ImageUrl = "/uploads/announcements/" + fileName
                    };
                    _context.AnnouncementImages.Add(announcementImage);
                }

                await _context.SaveChangesAsync();
                // Önce o binadaki tüm UserProfileId'leri çek
                var userRoles = await _context.UserBuildingRoles
                    .Where(ubr => ubr.BuildingId == announcement.BuildingId)
                    .Include(ubr => ubr.UserProfile)
                    .ThenInclude(up => up.IdentityUser) // UserProfile üzerinden ApplicationUser'a erişS
                    .ToListAsync();

                foreach (var userRole in userRoles)
                {
                    var message = $"Sayın kullanıcı, {announcement.BuildingId} numaralı binanızda yeni bir duyuru var: \"{announcement.Title}\".";

                    // İlgili linki hazırla (ör: Bina detay sayfasına veya duyuru detayına yönlendirebilirsin)
                    var link = Url.Action(
                         "Detail",          // Action adı
                         "Announcement",    // Controller adı
                         new { buildingId = announcement.BuildingId, id = announcement.Id },
                         protocol: Request.Scheme
                     );


                    // UserProfile üzerinden ApplicationUser ID'ye ulaş (ör: UserProfile içinde ApplicationUserId varsa)
                    // Eğer UserProfile doğrudan ApplicationUser ile ilişkiliyse:
                    var applicationUserId = userRole.UserProfile.IdentityUserId;

                    // Bildirimi gönder
                    await AddNotification(applicationUserId, message, link);
                }

            }


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

        // 📄 Sil
        [HttpGet("{id}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int buildingId, int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
                return NotFound();

            // Resimleri sil
            if (announcement.Images != null)
            {
                foreach (var img in announcement.Images)
                {
                    var filePath = Path.Combine(_env.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            ViewBag.BuildingId = buildingId; // Binaya göre filtreleme için BuildingId'yi ViewBag'e ekle

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int buildingId, int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
                return NotFound();

            ViewBag.BuildingId = buildingId;
            return View(announcement);
        }
        [HttpPost("{id}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int buildingId, int id, Announcement announcement, List<IFormFile> newImages)
        {
            if (ModelState.IsValid)
            {
                ViewBag.BuildingId = buildingId;
                return View(announcement);
            }

            var existingAnnouncement = await _context.Announcements
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == announcement.Id);

            if (existingAnnouncement == null)
                return NotFound();

            // Ana bilgileri güncelle
            existingAnnouncement.Title = announcement.Title;
            existingAnnouncement.Content = announcement.Content;
            existingAnnouncement.ExpireDate = announcement.ExpireDate;
            existingAnnouncement.IsImportant = announcement.IsImportant;

            // Yeni resimleri ekle
            if (newImages != null && newImages.Any())
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "announcements");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var image in newImages)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var newAnnouncementImage = new AnnouncementImage
                    {
                        AnnouncementId = existingAnnouncement.Id,
                        ImageUrl = "/uploads/announcements/" + fileName
                    };
                    _context.AnnouncementImages.Add(newAnnouncementImage);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { buildingId });
        }


        [HttpPost("{id}/Delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int buildingId, int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
                return NotFound();

            // Görselleri sil
            if (announcement.Images != null)
            {
                foreach (var img in announcement.Images)
                {
                    var filePath = Path.Combine(_env.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { buildingId });
        }

    }
}
