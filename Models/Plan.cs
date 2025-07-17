namespace YoneticiOtomasyonu.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } // Başlangıç, Standart, Kurumsal
        public string Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int MaxBuildings { get; set; }
        public int MaxUsers { get; set; }
    }

}
