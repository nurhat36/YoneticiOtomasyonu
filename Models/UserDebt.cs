using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoneticiOtomasyonu.Models
{
    public class UserDebt
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public int BuildingId { get; set; }
        public Building Building { get; set; }

        [Required(ErrorMessage = "Borç miktarı zorunludur.")]
        [Range(0.01, 1000000, ErrorMessage = "Borç miktarı pozitif olmalıdır.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } // Ör: "Aidat", "Elektrik", "Tamirat", "Diğer" vs.

        [StringLength(500)]
        public string? Description { get; set; }
        public int? UnitId { get; set; } // ✅ Yeni eklenen

        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PaidAt { get; set; }

        public Unit? Unit { get; set; } // ✅ Yeni eklenen, ilişkiyi temsil eder
    }
}
