namespace YoneticiOtomasyonu.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; }    // Bildirim kime ait
        public ApplicationUser User { get; set; }

        public string Message { get; set; }   // Bildirim metni

        public bool IsRead { get; set; } = false;  // Okundu bilgisi

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Link { get; set; }     // İstersen bildirime tıklanınca gidilecek url
    }

}
