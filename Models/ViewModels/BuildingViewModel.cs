using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class BuildingViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bina adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Bina adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Bina Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Adres zorunludur.")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        [Display(Name = "Tam Adres")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Bina türü seçilmelidir.")]
        [Display(Name = "Bina Türü")]
        public string Type { get; set; }

        public string? Block { get; set; }


        [Required(ErrorMessage = "Kat sayısı zorunludur.")]
        [Range(1, 100, ErrorMessage = "Kat sayısı 1-100 arasında olmalıdır.")]
        [Display(Name = "Kat Sayısı")]
        public int FloorCount { get; set; }

        [Required(ErrorMessage = "Birim sayısı zorunludur.")]
        [Range(1, 1000, ErrorMessage = "Birim sayısı 1-1000 arasında olmalıdır.")]
        [Display(Name = "Birim Sayısı (Daire/Oda)")]
        public int UnitCount { get; set; }

        [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Bina Görseli")]
        [DataType(DataType.Upload)]
        [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Sadece JPG, JPEG veya PNG formatında resim yükleyebilirsiniz.")]
        public IFormFile? ImageFile { get; set; }

        public string? CurrentImageUrl { get; set; }
    }
}