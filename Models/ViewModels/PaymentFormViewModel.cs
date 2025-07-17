using System.ComponentModel.DataAnnotations;

namespace YoneticiOtomasyonu.Models
{
    public class PaymentFormViewModel
    {
        public int PlanId { get; set; }
        public string CardHolderName { get; set; }
        [Required]
        [Display(Name = "Kart Numarası")]
        public string CardNumber { get; set; }

        [Required]
        [Display(Name = "Son Kullanma Ayı (MM)")]
        public string ExpireMonth { get; set; }

        [Required]
        [Display(Name = "Son Kullanma Yılı (YYYY)")]
        public string ExpireYear { get; set; }

        [Required]
        [Display(Name = "CVC")]
        public string Cvc { get; set; }

        // Yeni alanlar
        public string PlanName { get; set; }
        public string PlanDescription { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
    }

}
