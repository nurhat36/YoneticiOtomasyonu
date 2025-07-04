using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;

namespace YoneticiOtomasyonu.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env, ApplicationDbContext context)
        {
            _userManager = userManager;
            _env = env;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ProfiliDuzenle()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // UserProfile bilgilerini getir
            var userProfile = await _context.UserProfiles
               .Include(up => up.BuildingRoles)
               .ThenInclude(br => br.Building)
               .FirstOrDefaultAsync(up => up.IdentityUserId == user.Id);

            var model = new ProfileEditViewModel
            {
                // Identity bilgileri
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                CurrentProfileImageUrl = user.ProfileImageUrl ?? "/images/default-profile.png",

                // UserProfile bilgileri
                FullName = userProfile?.FullName,
                TCKN = userProfile?.TCKN,
                PhoneNumber2 = userProfile?.PhoneNumber2,
                Address = userProfile?.Address,
                EmergencyContact = userProfile?.EmergencyContact,
                RoleInBuilding = userProfile?.RoleInBuilding,
                BirthDate = userProfile?.BirthDate,
                BloodType = userProfile?.BloodType,
                Notes = userProfile?.Notes,
                BuildingRoles = userProfile.BuildingRoles.Select(br => new BuildingRoleViewModel
                {
                    BuildingId = br.BuildingId,
                    BuildingName = br.Building.Name,
                    Role = br.Role,
                    IsPrimary = br.IsPrimary
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfiliDuzenle(ProfileEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Profil fotoğrafı işlemleri
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profile-images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{user.Id}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(model.ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                // Eski resmi sil (varsa)
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.ProfileImageUrl = $"/uploads/profile-images/{uniqueFileName}";
            }

            // ApplicationUser güncellemeleri
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.UserName = model.UserName;

            var userUpdateResult = await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                foreach (var error in userUpdateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // UserProfile güncellemeleri
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.IdentityUserId == user.Id);

            if (userProfile == null)
            {
                // Eğer UserProfile yoksa yeni oluştur
                userProfile = new UserProfile
                {
                    IdentityUserId = user.Id,
                    FullName = model.FullName,
                    TCKN = model.TCKN,
                    PhoneNumber2 = model.PhoneNumber2,
                    Address = model.Address,
                    EmergencyContact = model.EmergencyContact,
                    RoleInBuilding = model.RoleInBuilding,
                    BirthDate = model.BirthDate,
                    BloodType = model.BloodType,
                    Notes = model.Notes
                };
                _context.UserProfiles.Add(userProfile);
            }
            else
            {
                // UserProfile varsa güncelle
                userProfile.FullName = model.FullName;
                userProfile.TCKN = model.TCKN;
                userProfile.PhoneNumber2 = model.PhoneNumber2;
                userProfile.Address = model.Address;
                userProfile.EmergencyContact = model.EmergencyContact;
                userProfile.RoleInBuilding = model.RoleInBuilding;
                userProfile.BirthDate = model.BirthDate;
                userProfile.BloodType = model.BloodType;
                userProfile.Notes = model.Notes;
                _context.UserProfiles.Update(userProfile);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Profil güncelleme sırasında bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }
        public async Task<IActionResult> MyDebts()
        {
            var userId = _userManager.GetUserId(User);

            var debts = await _context.UserDebts
                .Where(d => d.UserId == userId)
                .Include(d => d.Building)
                .Include(d => d.Unit)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(debts);
        }

    }
}
