using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YoneticiOtomasyonu.Models
{
    public class Building
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bina adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Bina adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Adres bilgisi zorunludur.")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Bina türü seçilmelidir.")]
        public string Type { get; set; } // Apartman, İş Merkezi, Site, Rezidans vb.

        public string? Block { get; set; } // Blok bilgisi, opsiyonel (null olabilir)


        [Range(1, 100, ErrorMessage = "Kat sayısı 1-100 arasında olmalıdır.")]
        public int FloorCount { get; set; }

        [Range(1, 1000, ErrorMessage = "Birim sayısı 1-1000 arasında olmalıdır.")]
        public int UnitCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        // Bina oluşturan yönetici (super admin)
        public string CreatorUserId { get; set; }
        public ApplicationUser CreatorUser { get; set; }

        // Navigation Properties
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<Income> Incomes { get; set; } = new List<Income>();
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
        public ICollection<WorkTask> WorkTasks { get; set; } = new List<WorkTask>();

        // Çoktan çoğa ilişki için
        public ICollection<UserBuildingRole> UserRoles { get; set; } = new List<UserBuildingRole>();
        public ICollection<DuesSetting> DuesSettings { get; set; } = new List<DuesSetting>();
        public ICollection<UserDebt> UserDebts { get; set; } = new List<UserDebt>();
    }
}