using YoneticiOtomasyonu.Models;
using System.Collections.Generic;

namespace YoneticiOtomasyonu.Models
{
    /// <summary>
    /// Toplantı bilgilerini tutan model sınıfı
    /// </summary>
    public class Meeting
    {
        public int Id { get; set; } // Toplantının benzersiz kimlik numarası

        /// <summary>
        /// Toplantının başlığı (Örnek: "Mayıs Ayı Olağan Toplantısı")
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Toplantının detaylı açıklaması
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Toplantının tarih ve saati
        /// </summary>
        public DateTime MeetingDate { get; set; }

        /// <summary>
        /// Toplantının yapılacağı yer (Örnek: "A Blok Toplantı Salonu")
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Toplantıda alınan kararlar (Toplantı sonrası doldurulur)
        /// </summary>
        public string? Decisions { get; set; } // Nullable (opsiyonel)

        /// <summary>
        /// Toplantı tutanağının dosya yolu (PDF veya DOCX dosyası)
        /// </summary>
        public string? MinutesFileUrl { get; set; } // Nullable (opsiyonel)

        // İLİŞKİLER (NAVIGATION PROPERTIES)

        /// <summary>
        /// Toplantının yapılacağı binanın ID'si
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Toplantının yapılacağı bina nesnesi
        /// </summary>
        public Building Building { get; set; }

        /// <summary>
        /// Toplantıyı düzenleyen kullanıcının ID'si
        /// </summary>
        public string OrganizedById { get; set; }

        /// <summary>
        /// Toplantıyı düzenleyen kullanıcı nesnesi
        /// </summary>
        public ApplicationUser OrganizedBy { get; set; }

        /// <summary>
        /// Bu toplantıya katılım bilgileri (Collection navigation property)
        /// </summary>
        public ICollection<MeetingAttendance> Attendances { get; set; }
    }

    /// <summary>
    /// Toplantı katılım bilgilerini tutan model sınıfı
    /// </summary>
    public class MeetingAttendance
    {
        public int Id { get; set; } // Katılım kaydının benzersiz kimlik numarası

        /// <summary>
        /// İlgili toplantının ID'si
        /// </summary>
        public int MeetingId { get; set; }

        /// <summary>
        /// İlgili toplantı nesnesi (Navigation property)
        /// </summary>
        public Meeting Meeting { get; set; }

        /// <summary>
        /// Katılımcı kullanıcının ID'si
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Katılımcı kullanıcı nesnesi (Navigation property)
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Katılım durumu (true: katılacak, false: katılmayacak)
        /// </summary>
        public bool WillAttend { get; set; }

        /// <summary>
        /// Katılım durumunun yanıtlandığı tarih
        /// </summary>
        public DateTime? ResponseDate { get; set; } // Nullable (otomatik doldurulabilir)

        /// <summary>
        /// Katılamama nedeni (WillAttend false ise doldurulur)
        /// </summary>
        public string? Excuse { get; set; } // Nullable (opsiyonel)
    }
}