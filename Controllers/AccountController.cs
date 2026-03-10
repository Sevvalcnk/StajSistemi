using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.Models;
using StajSistemi.Models.ViewModels;
using System.Security.Claims;

namespace StajSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // --- GİRİŞ (GET) ---
        [HttpGet]
        public IActionResult Login(string role)
        {
            ViewBag.SelectedRole = role;
            return View();
        }

        // --- GİRİŞ (POST) ---
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? role)
        {
            ViewBag.SelectedRole = role;
            if (ModelState.IsValid)
            {
                // Okul No veya Email ile kullanıcıyı bul
                var user = model.Email.Contains("@")
                    ? await _userManager.FindByEmailAsync(model.Email)
                    : await _userManager.FindByNameAsync(model.Email);

                if (user != null)
                {
                    // IP Kaydı (Hafta 3)
                    user.IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    await _userManager.UpdateAsync(user);

                    var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin")) return RedirectToAction("Index", "Admin");
                        if (roles.Contains("Advisor")) return RedirectToAction("Index", "Advisor");
                        return RedirectToAction("Index", "StudentPanel");
                    }
                }
                ModelState.AddModelError("", "Giriş bilgileri hatalı!");
            }
            return View(model);
        }

        // --- KAYIT (GET) ---
        [HttpGet]
        public IActionResult Register() => View();

        // --- KAYIT (POST) ---
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    // Öğrenciyse UserName = Okul No, Değilse Email
                    UserName = model.Role == "Student" ? model.StudentNo : model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    // SQL'deki StudentNo sütununu dolduruyoruz
                    StudentNo = model.Role == "Student" ? model.StudentNo : null
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // --- ŞİFREMİ UNUTTUM ---
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return View();
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { email = user.Email, token = token }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(email, "Şifre Sıfırlama", $"Şifrenizi yenilemek için lütfen <a href='{callbackUrl}'>buraya tıklayın</a>.");
            }
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        // --- ŞİFRE SIFIRLAMA ---
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (email == null || token == null) return RedirectToAction("Index", "Home");
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("Index", "Home");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded) return RedirectToAction("Login", new { role = "Student" });

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}