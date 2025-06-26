namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class BuildingRoleManagementViewModel
    {
        public int BuildingId { get; set; }
        public string BuildingName { get; set; }
        public List<UserRoleViewModel> CurrentRoles { get; set; }

        // Yeni rol atama
        public string NewUserId { get; set; }
        public string NewRole { get; set; }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTime AssignmentDate { get; set; }
    }
    
}
