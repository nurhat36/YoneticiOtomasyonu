using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YoneticiOtomasyonu.Data;
using YoneticiOtomasyonu.Models;
using YoneticiOtomasyonu.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext ekleniyor
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
// Identity servisi (rol destekli)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    // Ýstersen diðer Identity seçeneklerini buraya ekleyebilirsin.
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Razor Pages servisi (Identity Areas için gereklidir)
builder.Services.AddRazorPages();

// MVC Controller + View servisi
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Hata yönetimi ve HTTP ayarlarý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication ve Authorization middleware'leri
app.UseAuthentication();  // UseRouting sonrasý, UseAuthorization öncesi
app.UseAuthorization();

// Route ayarlarý
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Razor Pages map (Identity Areas için)
app.MapRazorPages();

app.Run();
