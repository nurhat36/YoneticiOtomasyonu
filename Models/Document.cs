namespace YoneticiOtomasyonu.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public string DocumentType { get; set; } // Sözleşme, Fatura, Kimlik vb.

        // İlişkiler
        public int? BuildingId { get; set; }
        public Building? Building { get; set; }

        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string UploadedById { get; set; }
        public ApplicationUser UploadedBy { get; set; }
    }
}