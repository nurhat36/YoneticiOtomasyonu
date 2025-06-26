using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class ProfileEditViewModel
    {
        // Identity ile ilgili alanlar
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Phone(ErrorMessage = "Geçersiz telefon formatı.")]
        [StringLength(11, ErrorMessage = "Telefon numarası 11 haneli olmalıdır.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
        public string UserName { get; set; }

        // Profil fotoğrafı
        [Display(Name = "Profil Fotoğrafı")]
        public IFormFile ProfileImage { get; set; }
        public string CurrentProfileImageUrl { get; set; }

        // UserProfile alanları
        [Display(Name = "Tam Ad")]
        [Required(ErrorMessage = "Tam ad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
        public string FullName { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        [StringLength(11, ErrorMessage = "TCKN 11 haneli olmalıdır.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "TCKN sadece rakamlardan oluşmalıdır.")]
        public string TCKN { get; set; }

        [Display(Name = "İkinci Telefon")]
        [Phone(ErrorMessage = "Geçersiz telefon formatı.")]
        [StringLength(11, ErrorMessage = "Telefon numarası 11 haneli olmalıdır.")]
        public string PhoneNumber2 { get; set; }

        [Display(Name = "Adres")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string Address { get; set; }

        [Display(Name = "Acil Durum İletişim")]
        [StringLength(100, ErrorMessage = "Acil durum iletişim en fazla 100 karakter olabilir.")]
        public string EmergencyContact { get; set; }

        [Display(Name = "Binadaki Rolü")]
        [StringLength(50, ErrorMessage = "Rol bilgisi en fazla 50 karakter olabilir.")]
        public string RoleInBuilding { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Kan Grubu")]
        [StringLength(5, ErrorMessage = "Kan grubu en fazla 5 karakter olabilir.")]
        public string BloodType { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
        public string Notes { get; set; }
        public List<BuildingRoleViewModel> BuildingRoles { get; set; }
    }
    public class BuildingRoleViewModel
    {
        public int BuildingId { get; set; }
        public string BuildingName { get; set; }
        public string Role { get; set; }
        public bool IsPrimary { get; set; }
    }
}
