using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using System.Threading.Tasks;
using YoneticiOtomasyonu.Services;
using YoneticiOtomasyonu.Models;
using Iyzipay.Request.V2.Subscription;

namespace YoneticiOtomasyonu.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IyzicoPaymentService _paymentService;

        public SubscriptionController(ApplicationDbContext context)
        {
            _context = context;
            _paymentService = new IyzicoPaymentService();
        }


        // Tüm planları göster
        public async Task<IActionResult> Index()
        {
            var plans = await _context.Plans.ToListAsync();
            return View(plans);
        }

        // Plan seçme (şimdilik ödeme olmadan seçildi gibi işleyelim)
        [HttpPost]
        public async Task<IActionResult> Select(int id)
        {
            var plan = await _context.Plans.FindAsync(id);
            if (plan == null)
                return NotFound();

            return RedirectToAction("Payment", "Subscription", new { id = id });
        }


        // Geçici ödeme ekranı (bir sonraki adımda gerçek iyzico entegrasyonu buraya gelecek)
        [HttpGet]
        public async Task<IActionResult> Payment(int id)
        {
            var plan = await _context.Plans.FindAsync(id);
            if (plan == null) return NotFound();

            var vm = new PaymentFormViewModel
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                PlanDescription = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                YearlyPrice = plan.YearlyPrice
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                return View("Payment", model); // Eğer doğrulama hatası varsa tekrar forma dön
            }

            var plan = await _context.Plans.FindAsync(model.PlanId);
            if (plan == null)
            {
                TempData["Error"] = "Plan bulunamadı.";
                return RedirectToAction("Payment", new { id = model.PlanId });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Payment", new { id = model.PlanId });
            }

            // Ödeme işlemini başlat
            var result = await _paymentService.MakePaymentAsync(user, plan, model); // model = kart bilgilerini içeriyor

            if (result.Status == "success")
            {
                // Başarılı ödeme kaydı yap
                var userPlan = new UserPlan
                {
                    UserId = user.Id,
                    PlanId = plan.Id,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(1),
                    IsActive = true
                };

                _context.UserPlans.Add(userPlan);
                await _context.SaveChangesAsync();

                return RedirectToAction("Success");
            }

            // Hata varsa
            TempData["Error"] = result.ErrorMessage ?? "Ödeme sırasında bir hata oluştu.";
            return RedirectToAction("Payment", new { id = model.PlanId });
        }


        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }

    }
}
