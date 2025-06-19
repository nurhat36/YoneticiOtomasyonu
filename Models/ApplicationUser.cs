using Microsoft.AspNetCore.Identity;

namespace YoneticiOtomasyonu.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public string ProfileImageUrl { get; set; }
    }
}
