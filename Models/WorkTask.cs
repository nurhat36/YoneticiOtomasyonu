namespace YoneticiOtomasyonu.Models
{
    public class WorkTask
    {
        public int Id { get; set; } // Benzersiz kimlik
        public string Title { get; set; } // Görev başlığı
        public string Description { get; set; } // Görev detayı
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Oluşturulma tarihi
        public DateTime? DueDate { get; set; } // Son teslim tarihi (opsiyonel)
        public string Status { get; set; } = "Beklemede"; // Durum: Beklemede, Devam Ediyor, Tamamlandı
        public string Priority { get; set; } = "Orta"; // Öncelik: Düşük, Orta, Yüksek

        // İlişkiler
        public int? ComplaintId { get; set; } // Bağlı olduğu şikayet ID (opsiyonel)
        public Complaint? Complaint { get; set; } // Bağlı olduğu şikayet nesnesi

        public int BuildingId { get; set; } // Bağlı olduğu bina ID
        public Building Building { get; set; } // Bağlı olduğu bina nesnesi

        public string CreatedById { get; set; } // Görevi oluşturan kullanıcı ID
        public ApplicationUser CreatedBy { get; set; } // Görevi oluşturan kullanıcı nesnesi

        public string AssignedToId { get; set; } // Görev atanan kullanıcı ID
        public ApplicationUser AssignedTo { get; set; } // Görev atanan kullanıcı nesnesi
    }
}