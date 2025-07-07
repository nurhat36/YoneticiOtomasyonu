using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Hubs;

namespace YoneticiOtomasyonu.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByIdAsync(userId);

            if (targetUser == null)
            {
                return NotFound("Kullanıcı bulunamadı");
            }

            // Mesajları getir
            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Okunmamış mesajları işaretle
            var unreadMessages = messages.Where(m => m.ReceiverId == currentUser.Id && !m.IsRead);
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();

            ViewBag.CurrentUser = currentUser;
            ViewBag.TargetUser = targetUser;

            return View(messages);
        }

        // MessageController'a eklenmesi gereken methodlar

        [HttpGet]
        public async Task<IActionResult> GetMessages(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                           (m.SenderId == userId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    senderId = m.SenderId,
                    senderName = m.Sender.UserName,
                    content = m.Content,
                    sentAt = m.SentAt,
                    isRead = m.IsRead
                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(string senderId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCounts()
        {
            var currentUserId = _userManager.GetUserId(User);

            var unreadCounts = await _context.Messages
                .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new
                {
                    senderId = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return Json(unreadCounts);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageModel model)
        {
            try
            {
                var sender = await _userManager.GetUserAsync(User);
                var receiver = await _userManager.FindByIdAsync(model.ReceiverId);

                if (receiver == null)
                {
                    return NotFound(new { success = false, message = "Alıcı bulunamadı" });
                }

                var message = new Message
                {
                    SenderId = sender.Id,
                    ReceiverId = receiver.Id,
                    Content = model.Content,
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // SignalR ile mesaj gönder
                await _hubContext.Clients.User(receiver.Id).SendAsync("ReceiveMessage", new
                {
                    SenderId = sender.Id,
                    SenderName = sender.UserName,
                    Content = model.Content,
                    SentAt = message.SentAt.ToString("g")
                });

                return Ok(new { success = true, message = "Mesaj gönderildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyManagers()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Kullanıcının UserProfile'ını bul
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(x => x.IdentityUserId == currentUser.Id);

            if (userProfile == null)
            {
                return Json(Array.Empty<object>());
            }

            // Kullanıcının bağlı olduğu bina id'leri
            var buildingIds = await _context.UserBuildingRoles
                .Where(x => x.UserProfileId == userProfile.Id)
                .Select(x => x.BuildingId)
                .ToListAsync();

            // O binalardaki yöneticilerin UserProfile'larını al
            var managerProfiles = await _context.UserBuildingRoles
                .Include(x => x.UserProfile)
                .ThenInclude(up => up.IdentityUser)
                .Where(x => buildingIds.Contains(x.BuildingId) && x.Role == "Yönetici")
                .Select(x => x.UserProfile)
                .Distinct()
                .ToListAsync();

            var result = managerProfiles
                .Where(up => up.IdentityUser != null)
                .Select(up => new {
                    id = up.IdentityUserId,
                    name = up.IdentityUser.UserName,
                    profileImage = up.IdentityUser.ProfileImageUrl ?? "/images/default-profile.png"
                });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            var count = await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .CountAsync();

            return Json(new { count });
        }
    }
    public class MessageModel
    {
        public string ReceiverId { get; set; }
        public string Content { get; set; }
    }
}