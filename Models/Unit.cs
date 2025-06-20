namespace YoneticiOtomasyonu.Models
{
    public class Unit
    {
        public int Id { get; set; } // Benzersiz kimlik
        public string Number { get; set; } // Birim numarası (Örnek: "Daire 5" veya "Oda 201")
        public string Type { get; set; } // Birim türü: Daire, Ofis, Sınıf, Oda vb.
        public int Floor { get; set; } // Bulunduğu kat numarası
        public decimal Area { get; set; } // Metrekare (m²) cinsinden alan
        public bool IsOccupied { get; set; } // Dolu/Boş durumu
        public string? Description { get; set; } // Birim açıklaması (opsiyonel)

        // İlişkiler
        public int BuildingId { get; set; } // Bağlı olduğu bina ID
        public Building Building { get; set; } // Bağlı olduğu bina nesnesi

        public string? ResidentId { get; set; } // Birimde kalan/çalışan kullanıcı ID (opsiyonel)
        public ApplicationUser? Resident { get; set; } // Birimde kalan/çalışan kullanıcı nesnesi

        public ICollection<Complaint> Complaints { get; set; } // Bu birime ait şikayetler
    }
}