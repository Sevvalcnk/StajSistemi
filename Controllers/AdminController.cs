using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.Models;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AdminController(UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            // Burası genellikle admin ana sayfasıdır, istersen öğrenci listesine yönlendirebilirsin
            return View();
        }

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(AppUser model)
        {
            // 1. Şifre Üretme: GUID'deki çizgileri kaldırıp daha güvenli hale getirdik
            string rawGuid = Guid.NewGuid().ToString().Replace("-", "");
            string temporaryPassword = "Staj" + rawGuid.Substring(0, 5).ToUpper() + "1!";

            // 2. Yeni Kullanıcı Hazırlığı
            var newStudent = new AppUser
            {
                UserName = model.UserName, // Okul No
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            // 3. Veritabanına Kayıt
            var result = await _userManager.CreateAsync(newStudent, temporaryPassword);

            if (result.Succeeded)
            {
                // 4. Rol Atama
                await _userManager.AddToRoleAsync(newStudent, "Student");

                // 5. Mail Şablonu (Daha şık ve garanti)
                string subject = "Staj Takip Sistemi - Giriş Bilgileriniz";
                string message = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2 style='color: #2c3e50;'>Merhaba {newStudent.FullName},</h2>
                        <p>Staj takip sistemine kaydınız başarıyla tamamlanmıştır.</p>
                        <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>Kullanıcı Adınız (Okul No):</strong> {newStudent.UserName}</p>
                            <p><strong>Geçici Şifreniz:</strong> <span style='color: #d35400; font-size: 18px; font-weight: bold;'>{temporaryPassword}</span></p>
                        </div>
                        <p style='color: #7f8c8d; font-size: 12px;'>Lütfen giriş yaptıktan sonra profilinizden şifrenizi güncelleyiniz.</p>
                    </div>";

                try
                {
                    await _emailSender.SendEmailAsync(newStudent.Email, subject, message);
                    TempData["SuccessMessage"] = $"{newStudent.FullName} başarıyla eklendi. Şifre: {temporaryPassword}"; // Admin panelinde de görsün diye ekledik
                }
                catch (Exception ex)
                {
                    // Mail gitmese bile en azından admin şifreyi ekranda görebilsin diye TempData'ya ekledik
                    TempData["WarningMessage"] = $"Öğrenci eklendi fakat mail gönderilemedi. Şifre: {temporaryPassword}";
                }

                // StudentController'daki Index'e (Öğrenci listesi) yönlendiriyoruz
                return RedirectToAction("Index", "Student");
            }

            // Hata Durumu (Şifre politikasına uymazsa veya kullanıcı zaten varsa)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}