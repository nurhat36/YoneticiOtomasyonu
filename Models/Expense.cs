namespace YoneticiOtomasyonu.Models
{
    public class Expense
    {
        public int Id { get; set; } // Benzersiz kimlik
        public decimal Amount { get; set; } // Gider miktarı (TL cinsinden)
        public DateTime Date { get; set; } // Gider tarihi
        public string Type { get; set; } // Gider türü: Temizlik, Bakım, Elektrik, Su vb.
        public string Description { get; set; } // Gider açıklaması
        public string PaymentMethod { get; set; } // Ödeme yöntemi
        public string? ReceiptNumber { get; set; } // Fiş/fatura numarası (opsiyonel)
        public string? ReceiptImageUrl { get; set; } // Fiş/fatura resmi URL (opsiyonel)

        // İlişkiler
        public int BuildingId { get; set; } // Bağlı olduğu bina ID
        public Building Building { get; set; } // Bağlı olduğu bina nesnesi

        public string? RecordedById { get; set; } // Kaydı yapan kullanıcı ID (opsiyonel)
        public ApplicationUser? RecordedBy { get; set; } // Kaydı yapan kullanıcı nesnesi
    }
}