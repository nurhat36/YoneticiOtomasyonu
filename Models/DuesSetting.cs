using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoneticiOtomasyonu.Models
{
    public class DuesSetting
    {
        public int Id { get; set; }

        [Required]
        public int BuildingId { get; set; }
        public Building Building { get; set; }

        [Required(ErrorMessage = "Aidat miktarı zorunludur.")]
        [Range(0.01, 1000000, ErrorMessage = "Aidat miktarı pozitif olmalıdır.")]
        public decimal Amount { get; set; }

        [Range(1, 28, ErrorMessage = "Toplanma günü 1-28 arasında olmalıdır.")]
        public int CollectionDay { get; set; } // Ayın hangi günü toplanacağı

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
