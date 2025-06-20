namespace YoneticiOtomasyonu.Models
{
    public class Complaint
    {
        public int Id { get; set; } // Benzersiz kimlik
        public string Title { get; set; } // Şikayet başlığı
        public string Description { get; set; } // Şikayet detayı
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Oluşturulma tarihi
        public string Status { get; set; } = "Beklemede"; // Durum: Beklemede, Atandı, Çözüldü, Reddedildi
        public string? ImageUrl { get; set; } // Şikayet resmi URL (opsiyonel)
        public string? Response { get; set; } // Yönetici cevabı (opsiyonel)
        public DateTime? ResponseDate { get; set; } // Cevaplanma tarihi (opsiyonel)

        // İlişkiler
        public int UnitId { get; set; } // Bağlı olduğu birim ID
        public Unit Unit { get; set; } // Bağlı olduğu birim nesnesi

        public string ComplainantId { get; set; } // Şikayet eden kullanıcı ID
        public ApplicationUser Complainant { get; set; } // Şikayet eden kullanıcı nesnesi

        public string? AssignedToId { get; set; } // Atanan personel ID (opsiyonel)
        public ApplicationUser? AssignedTo { get; set; } // Atanan personel nesnesi

        public ICollection<WorkTask> WorkTasks { get; set; } // Bu şikayetle ilişkili görevler
    }
}