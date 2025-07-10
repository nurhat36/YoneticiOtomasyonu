using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models.ViewModels;
namespace YoneticiOtomasyonu.Controllers
{
    [Route("Buildings/{buildingId:int}/[controller]/[action]")]
    public class DuesSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DuesSettingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int buildingId)
        {
            var duesSettings = await _context.DuesSettings
                .Where(ds => ds.BuildingId == buildingId)
                .Include(ds => ds.Building)
                .Select(ds => new DuesSettingViewModel
                {
                    Id = ds.Id,
                    BuildingId = ds.BuildingId,
                    BuildingName = ds.Building.Name,
                    Amount = ds.Amount,
                    CollectionDay = ds.CollectionDay,
                    Description = ds.Description
                })
                .ToListAsync();

            ViewBag.BuildingId = buildingId; // View'da kullanmak için
            return View(duesSettings);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var duesSetting = await _context.DuesSettings.FindAsync(id);
            if (duesSetting == null)
            {
                return NotFound();
            }

            ViewBag.BuildingSelectList = new SelectList(_context.Buildings, "Id", "Name", duesSetting.BuildingId);


            return View(duesSetting);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, DuesSetting duesSetting)
        {
            if (id != duesSetting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(duesSetting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DuesSettingExists(duesSetting.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Buildings = _context.Buildings.ToList();
            return View(duesSetting);
        }

        private bool DuesSettingExists(int id)
        {
            return _context.DuesSettings.Any(e => e.Id == id);
        }



        public IActionResult Create(int buildingId)
        {
            var building = _context.Buildings.FirstOrDefault(b => b.Id == buildingId);
            if (building == null)
            {
                return NotFound();
            }

            var duesSetting = new DuesSetting
            {
                BuildingId = buildingId
            };

            ViewBag.BuildingName = building.Name;
            return View(duesSetting);
        }


        [HttpPost]
        public async Task<IActionResult> Create(DuesSetting duesSetting)
        {
            if (!ModelState.IsValid)
            {
                _context.DuesSettings.Add(duesSetting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { buildingId = duesSetting.BuildingId });
            }

            var building = await _context.Buildings.FindAsync(duesSetting.BuildingId);
            ViewBag.BuildingName = building?.Name;
            return View(duesSetting);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var duesSetting = await _context.DuesSettings
                .Include(ds => ds.Building)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (duesSetting == null)
            {
                return NotFound();
            }

            return View(duesSetting);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var duesSetting = await _context.DuesSettings.FindAsync(id);
            if (duesSetting != null)
            {
                _context.DuesSettings.Remove(duesSetting);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> AssignDues(int buildingId)
        {
            var duesSetting = await _context.DuesSettings.FirstOrDefaultAsync(d => d.BuildingId == buildingId);
            if (duesSetting == null)
            {
                return NotFound();
            }

            var now = DateTime.Now;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // Bu ay zaten borç atandı mı?
            var existingDebts = await _context.UserDebts
                .Where(ud => ud.BuildingId == buildingId
                             && ud.Type == "Aidat"
                             && ud.CreatedAt.Month == currentMonth
                             && ud.CreatedAt.Year == currentYear)
                .AnyAsync();

            if (existingDebts)
            {
                TempData["Message"] = "Bu ay için zaten aidatlar atanmış!";
                return RedirectToAction("Index", new { buildingId = buildingId });
            }

            // Her daireye ayrı ayrı borç atama
            var units = await _context.Units
                .Where(u => u.BuildingId == buildingId && u.ResidentId != null)
                .ToListAsync();

            foreach (var unit in units)
            {
                var debt = new UserDebt
                {
                    UserId = unit.ResidentId,
                    BuildingId = buildingId,
                    Amount = duesSetting.Amount,
                    Type = "Aidat",
                    CreatedAt = DateTime.Now,
                    IsPaid = false,
                    UnitId = unit.Id // Eğer tablon varsa
                };

                _context.UserDebts.Add(debt);
                string message = $"Sayın kullanıcı, sizi {debt.BuildingId} numaralı bina  daire {debt.UnitId} Ödenmemiş bir adet aidatınız bulunmaktadır lütfen en kısa zamanda ödeyiniz.";
                string link = Url.Action("MyDebts", "Profile");

                await AddNotification(debt.UserId, message, link);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Aidatlar başarıyla atandı!";
            return RedirectToAction("Index", new { buildingId = buildingId });
        }
        private async Task AddNotification(string userId, string message, string? link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Link = link,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> BuildingDues(int buildingId)
        {
            var userId = _userManager.GetUserId(User);

            // UserProfile'ı bul
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Unauthorized();
            }

            // Yönetici mi kontrol et
            var isManager = await _context.UserBuildingRoles
                .AnyAsync(r => r.BuildingId == buildingId && r.UserProfileId == userProfile.Id && r.Role == "Yönetici");

            if (!isManager)
            {
                return Unauthorized();
            }



            var debts = await _context.UserDebts
                .Where(d => d.BuildingId == buildingId && d.Type == "Aidat")
                .Include(d => d.User)
                .Include(d => d.Unit)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            ViewBag.BuildingId = buildingId;

            return View(debts);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id, int buildingId)
        {
            var userId = _userManager.GetUserId(User);

            // UserProfile'ı bul
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Unauthorized();
            }

            // Yönetici mi kontrol et
            var isManager = await _context.UserBuildingRoles
                .AnyAsync(r => r.BuildingId == buildingId && r.UserProfileId == userProfile.Id && r.Role == "Yönetici");

            if (!isManager)
            {
                return Unauthorized();
            }

            var debt = await _context.UserDebts.FindAsync(id);
            if (debt == null)
            {
                return NotFound();
            }

            debt.IsPaid = true;
            debt.PaidAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("BuildingDues", new { buildingId = buildingId });
        }
        public async Task<IActionResult> ConfirmPayment(int debtId, int buildingId)
        {
            var userId = _userManager.GetUserId(User);

            // Kullanıcının profilini al
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Unauthorized();
            }

            var isManager = await _context.UserBuildingRoles
                .AnyAsync(r => r.BuildingId == buildingId && r.UserProfileId == userProfile.Id && r.Role == "Yönetici");

            if (!isManager)
            {
                return Unauthorized();
            }

            var debt = await _context.UserDebts
                .Include(d => d.User)
                .Include(d => d.Unit)
                .FirstOrDefaultAsync(d => d.Id == debtId);

            if (debt == null)
            {
                return NotFound();
            }

            // View'a borç bilgisi gönder
            return View(debt);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int debtId, int buildingId, string paymentMethod, string description)
        {
            var userId = _userManager.GetUserId(User);

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (userProfile == null)
            {
                return Unauthorized();
            }

            var isManager = await _context.UserBuildingRoles
                .AnyAsync(r => r.BuildingId == buildingId && r.UserProfileId == userProfile.Id && r.Role == "Yönetici");

            if (!isManager)
            {
                return Unauthorized();
            }

            var debt = await _context.UserDebts
                .Include(d => d.Unit)
                .FirstOrDefaultAsync(d => d.Id == debtId);

            if (debt == null)
            {
                return NotFound();
            }

            // Borcu ödenmiş olarak işaretle
            debt.IsPaid = true;

            // Gelir kaydı oluştur
            var income = new Income
            {
                Amount = debt.Amount,
                Date = DateTime.Now,
                Type = "Aidat",
                Description = description,
                PaymentMethod = paymentMethod,
                BuildingId = buildingId,
                UnitId = debt.UnitId,
                PayerId = debt.UserId,
                RecordedById = userId
            };

            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();

            return RedirectToAction("BuildingDues", new { buildingId = buildingId });
        }





    }
}
