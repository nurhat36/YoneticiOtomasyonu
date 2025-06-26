namespace YoneticiOtomasyonu.Models
{
    public class UserProfile
    {
        public int Id { get; set; } // Benzersiz kimlik
        public string IdentityUserId { get; set; } // Identity kullanıcı ID'si
        public ApplicationUser IdentityUser { get; set; } // Identity kullanıcı nesnesi

        public string FullName { get; set; } // Kullanıcının tam adı
        public string? TCKN { get; set; } // T.C. Kimlik No (opsiyonel)
        public string? PhoneNumber2 { get; set; } // İkinci telefon no (opsiyonel)
        public string? Address { get; set; } // Adres bilgisi (opsiyonel)
        public string? EmergencyContact { get; set; } // Acil durum kontağı (opsiyonel)
        public string? RoleInBuilding { get; set; } // Binadaki rolü: Yönetici, Kiracı, Görevli vb.

        public DateTime? BirthDate { get; set; } // Doğum tarihi (opsiyonel)
        public string? BloodType { get; set; } // Kan grubu (opsiyonel)
        public string? Notes { get; set; } // Ek notlar (opsiyonel)
        public ICollection<UserBuildingRole> BuildingRoles { get; set; }
    }
}