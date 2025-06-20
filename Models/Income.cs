namespace YoneticiOtomasyonu.Models
{
    public class Income
    {
        public int Id { get; set; } // Benzersiz kimlik
        public decimal Amount { get; set; } // Gelir miktarı (TL cinsinden)
        public DateTime Date { get; set; } // Gelir tarihi
        public string Type { get; set; } // Gelir türü: Kira, Aidat, Bağış vb.
        public string Description { get; set; } // Gelir açıklaması
        public string PaymentMethod { get; set; } // Ödeme yöntemi: Nakit, Kredi Kartı, Havale vb.

        // İlişkiler
        public int BuildingId { get; set; } // Bağlı olduğu bina ID
        public Building Building { get; set; } // Bağlı olduğu bina nesnesi

        public int? UnitId { get; set; } // Bağlı olduğu birim ID (opsiyonel)
        public Unit? Unit { get; set; } // Bağlı olduğu birim nesnesi

        public string? PayerId { get; set; } // Ödeme yapan kullanıcı ID (opsiyonel)
        public ApplicationUser? Payer { get; set; } // Ödeme yapan kullanıcı nesnesi

        public string? RecordedById { get; set; } // Kaydı yapan kullanıcı ID (opsiyonel)
        public ApplicationUser? RecordedBy { get; set; } // Kaydı yapan kullanıcı nesnesi
    }
}