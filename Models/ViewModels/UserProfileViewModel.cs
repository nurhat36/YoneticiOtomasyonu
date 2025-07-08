namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string FullName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Slug { get; set; }
        public DateTime? LastActiveAt { get; set; }

        public bool IsOnline => LastActiveAt.HasValue && LastActiveAt.Value > DateTime.UtcNow.AddMinutes(-5);
        public int ManagerBuildingCount { get; set; }
    }

}
