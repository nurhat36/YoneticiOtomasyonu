// ViewModel
namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Slug { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public bool IsOnline => LastActiveAt.HasValue && LastActiveAt.Value > DateTime.UtcNow.AddMinutes(-5);
        public int ManagerBuildingCount { get; set; }

        // UserProfile'dan gelen ek bilgiler
        public string PhoneNumber2 { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public string RoleInBuilding { get; set; }
        public string BloodType { get; set; }
    }
}