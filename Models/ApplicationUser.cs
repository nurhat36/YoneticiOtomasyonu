using Microsoft.AspNetCore.Identity;

namespace YoneticiOtomasyonu.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public string ProfileImageUrl { get; set; }
        public string Slug { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public ICollection<UserDebt> UserDebts { get; set; } = new List<UserDebt>();

    }
}
