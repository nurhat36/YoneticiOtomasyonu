namespace YoneticiOtomasyonu.Models
{
    public class BuildingPersonnel
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public int BuildingId { get; set; }
        public string DutyType { get; set; }
        public decimal MonthlySalary { get; set; }
        public bool IsActive { get; set; }
        public DateTime EmploymentStartDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public string Notes { get; set; }

        // Navigation properties
        public ApplicationUser ApplicationUser { get; set; }
        public Building Building { get; set; }
    }

}
