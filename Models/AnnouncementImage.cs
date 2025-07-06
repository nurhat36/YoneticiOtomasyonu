namespace YoneticiOtomasyonu.Models
{
    public class AnnouncementImage
    {
        public int Id { get; set; } // Benzersiz kimlik

        public string ImageUrl { get; set; } // Resim yolu

        // İlişkiler
        public int AnnouncementId { get; set; } // Bağlı olduğu duyuru ID
        public Announcement Announcement { get; set; } // Duyuru nesnesi
    }
}
