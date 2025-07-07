using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using YoneticiOtomasyonu.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using YoneticiOtomasyonu.Data;

namespace YoneticiOtomasyonu.ViewComponents
{
    public class ChatBoxViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ChatBoxViewComponent(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
            {
                return Content(string.Empty);
            }

            ViewData["ProfilePic"] = currentUser.ProfileImageUrl ?? "/images/default-profile.png";
            ViewData["UserName"] = currentUser.UserName;
            ViewData["CurrentUserId"] = currentUser.Id;

            // Kullanıcının mesajlaştığı kişileri ve okunmamış mesaj sayılarını al
            var recentContacts = await GetRecentContactsWithUnreadCounts(currentUser.Id);

            return View(recentContacts);
        }

        private async Task<List<RecentContactViewModel>> GetRecentContactsWithUnreadCounts(string userId)
        {
            // Son mesajlaştığı kişileri al (en son mesajlaştığı 5 kişi)
            var recentContacts = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .Take(5)
                .ToListAsync();

            var result = new List<RecentContactViewModel>();

            foreach (var contactId in recentContacts)
            {
                var user = await _userManager.FindByIdAsync(contactId);
                if (user != null)
                {
                    var unreadCount = await _context.Messages
                        .CountAsync(m => m.SenderId == contactId &&
                                       m.ReceiverId == userId &&
                                       !m.IsRead);

                    result.Add(new RecentContactViewModel
                    {
                        UserId = contactId,
                        UserName = user.UserName,
                        FullName = user.UserName,
                        ProfileImageUrl = user.ProfileImageUrl ?? "/images/default-profile.png",
                        UnreadCount = unreadCount,
                        LastMessageDate = _context.Messages
                            .Where(m => (m.SenderId == userId && m.ReceiverId == contactId) ||
                                        (m.SenderId == contactId && m.ReceiverId == userId))
                            .Max(m => m.SentAt)
                    });
                }
            }

            // Son mesaj tarihine göre sırala
            return result.OrderByDescending(c => c.LastMessageDate).ToList();
        }
    }

    public class RecentContactViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ProfileImageUrl { get; set; }
        public int UnreadCount { get; set; }
        public DateTime LastMessageDate { get; set; }
    }
}