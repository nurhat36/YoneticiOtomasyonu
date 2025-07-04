namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class DuesSettingViewModel
    {
        public int Id { get; set; }

        public int BuildingId { get; set; }
        public string BuildingName { get; set; }

        public decimal Amount { get; set; }
        public int CollectionDay { get; set; }
        public string? Description { get; set; }
    }

}
