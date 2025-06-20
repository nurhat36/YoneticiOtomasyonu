namespace YoneticiOtomasyonu.Models
{
    public class Building
    {
        public int Id { get; set; } // Benzersiz kimlik
        public string Name { get; set; } // Bina adı (Örnek: "Güneş Apartmanı")
        public string Address { get; set; } // Bina adresi (tam adres)
        public string Type { get; set; } // Bina türü: Okul, Yurt, İşyeri, Apartman vb.
        public int FloorCount { get; set; } // Kat sayısı
        public int UnitCount { get; set; } // Birim sayısı (daire/ofis/oda sayısı)
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Oluşturulma tarihi
        public string? Description { get; set; } // Bina açıklaması (opsiyonel)
        public string? ImageUrl { get; set; } // Bina resmi URL (opsiyonel)

        // İlişkiler (Navigation Properties)
        public ICollection<Unit> Units { get; set; } // Binadaki birimler (daireler/odalar)
        public ICollection<Expense> Expenses { get; set; } // Binaya ait giderler
        public ICollection<Income> Incomes { get; set; } // Binaya ait gelirler
        public ICollection<Announcement> Announcements { get; set; } // Binaya ait duyurular
        public ICollection<Meeting> Meetings { get; set; }
        public ICollection<WorkTask> WorkTasks { get; set; }
    }
}