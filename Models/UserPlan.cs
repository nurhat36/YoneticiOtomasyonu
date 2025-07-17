namespace YoneticiOtomasyonu.Models
{
    public class UserPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Identity user
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        public ApplicationUser User { get; set; }
        public Plan Plan { get; set; }
    }

}
