using YoneticiOtomasyonu.Models;
using System;

namespace YoneticiOtomasyonu.Models.ViewModels
{
    public class BuildingWithRoleViewModel
    {
        public Building Building { get; set; }
        public string Role { get; set; } // "Yönetici", "Kiracı", "Görevli" etc.

        // Additional useful properties you might need:
        public bool CanEdit => Role == "Yönetici";
        public bool CanDelete => Role == "Yönetici";
        public bool CanManageUsers => Role == "Yönetici";
        public string RoleBadgeClass
        {
            get
            {
                return Role switch
                {
                    "Yönetici" => "badge-success",
                    "Kiracı" => "badge-primary",
                    "Görevli" => "badge-info",
                    _ => "badge-secondary"
                };
            }
        }

        public string BuildingTypeBadgeClass
        {
            get
            {
                return Building.Type switch
                {
                    "Apartman" => "badge-primary",
                    "Site" => "badge-success",
                    "İş Merkezi" => "badge-info",
                    "Rezidans" => "badge-warning",
                    _ => "badge-secondary"
                };
            }
        }
    }
}