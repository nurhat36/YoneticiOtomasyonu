namespace YoneticiOtomasyonu.Models
{
    public class ComplaintImage
    {
        public int Id { get; set; }
        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; }

        public string ImageUrl { get; set; }  // Resim URL’si (kaydedilen dosyanın yolu)
    }

}
