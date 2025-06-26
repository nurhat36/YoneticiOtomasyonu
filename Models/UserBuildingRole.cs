
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace YoneticiOtomasyonu.Models
    {
        public class UserBuildingRole
        {
            public int Id { get; set; }

            [ForeignKey("UserProfile")]
            public int UserProfileId { get; set; }
            public UserProfile UserProfile { get; set; }

            [ForeignKey("Building")]
            public int BuildingId { get; set; }
            public Building Building { get; set; }

            [Required]
            [StringLength(50)]
            public string Role { get; set; } // "Yönetici", "Kiracı", "Görevli", "Site Sakini"

            public bool IsPrimary { get; set; } // Kullanıcının ana binası mı?
            public DateTime AssignmentDate { get; set; } = DateTime.Now;

            [ForeignKey("AssignedByUser")]
            public string AssignedByUserId { get; set; } // Rolü atayan
            public ApplicationUser AssignedByUser { get; set; }
        }
    }
