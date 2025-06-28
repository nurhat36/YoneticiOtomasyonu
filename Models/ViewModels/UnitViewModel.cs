namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class UnitViewModel
    {
        public int Id { get; set; }

        public string Number { get; set; }

        public string Type { get; set; }

        public int Floor { get; set; }

        public decimal Area { get; set; }

        public bool IsOccupied { get; set; }

        public string? Description { get; set; }

        public int BuildingId { get; set; }

        public string? ResidentId { get; set; } // Seçilen kullanıcı ID

        public List<ApplicationUser>? Residents { get; set; } // Kullanıcı listesini buraya alacağız
        

    }
    
}
