namespace YoneticiOtomasyonu.Models
{
    public class Announcement
    {
        public int Id { get; set; } // Benzersiz kimlik
        public string Title { get; set; } // Duyuru başlığı
        public string Content { get; set; } // Duyuru içeriği
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Oluşturulma tarihi
        public DateTime? ExpireDate { get; set; } // Son geçerlilik tarihi (opsiyonel)
        public bool IsImportant { get; set; } // Aciliyet durumu (true/false)
        public string? ImageUrl { get; set; } // Duyuru resmi URL (opsiyonel)

        // İlişkiler
        public int BuildingId { get; set; } // Bağlı olduğu bina ID
        public Building Building { get; set; } // Bağlı olduğu bina nesnesi

        public string AuthorId { get; set; } // Duyuruyu oluşturan kullanıcı ID
        public ApplicationUser Author { get; set; } // Duyuruyu oluşturan kullanıcı nesnesi
    }
}