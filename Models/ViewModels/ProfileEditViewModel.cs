using System.ComponentModel.DataAnnotations;

namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class ProfileEditViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Phone(ErrorMessage = "Geçersiz telefon formatı.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public IFormFile ProfileImage { get; set; }

        public string CurrentProfileImageUrl { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }
    }
}

