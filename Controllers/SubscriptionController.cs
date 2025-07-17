using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using System.Threading.Tasks;
using YoneticiOtomasyonu.Services;
using YoneticiOtomasyonu.Models;

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

            // Ödeme ekranına yönlendirme yapılacak (henüz entegre etmedik)
            TempData["SelectedPlanId"] = id;
            return RedirectToAction("Payment", "Subscription");
        }

        // Geçici ödeme ekranı (bir sonraki adımda gerçek iyzico entegrasyonu buraya gelecek)
        public async Task<IActionResult> Payment()
        {
            var planId = TempData["SelectedPlanId"];
            if (planId == null)
                return RedirectToAction("Index");

            var plan = await _context.Plans.FindAsync((int)planId);
            return View(plan);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int planId)
        {
            var plan = await _context.Plans.FindAsync(planId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            var result = await _paymentService.MakePaymentAsync(user, plan);

            if (result.Status == "success")
            {
                // Ödeme başarılı, UserPlan kaydı yap
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

            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Payment", new { id = planId });
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
