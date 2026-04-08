using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.Models;
using StajSistemi.Models.ViewModels;
using StajSistemi.data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http; // ✅ Session işlemleri için mühürlü

namespace StajSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string role)
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["LoginWarning"] = "Zaten aktif bir oturumunuz bulunuyor.";
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
                if (User.IsInRole("Advisor")) return RedirectToAction("Index", "Advisor");
                return RedirectToAction("Index", "StudentPanel");
            }
            ViewBag.SelectedRole = role ?? "Student";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? role)
        {
            ViewBag.SelectedRole = role;
            if (ModelState.IsValid)
            {
                var user = model.Email.Contains("@")
                    ? await _userManager.FindByEmailAsync(model.Email)
                    : await _userManager.FindByNameAsync(model.Email);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        // --- ✅ HAFTA 3 MÜHÜRÜ: IP ADRESİ VE LOG KAYDI ---
                        string remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                        // 1. Session'a Mühürle (Hızlı Erişim)
                        HttpContext.Session.SetString("UserIP", remoteIp);
                        HttpContext.Session.SetString("UserEmail", user.Email ?? "");

                        // 2. Veritabanına Mühürle (Kalıcı Kayıt - LoginLogs)
                        // Senin LoginLog modelindeki isimlere (Username, LoginTime) göre ayarladık
                        var log = new LoginLog
                        {
                            Username = user.UserName,
                            IpAddress = remoteIp,
                            LoginTime = DateTime.Now
                        };
                        _context.LoginLogs.Add(log);
                        await _context.SaveChangesAsync();
                        // ------------------------------------------------

                        TempData.Remove("LoginWarning");
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

        [HttpGet]
        public IActionResult Register(string role)
        {
            ViewBag.SelectedRole = role ?? "Student";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var nameParts = model.FullName.Trim().Split(' ');
                string firstName = nameParts[0];
                string lastName = nameParts.Length > 1 ? nameParts[nameParts.Length - 1] : "";

                var user = new AppUser
                {
                    UserName = model.Role == "Student" ? model.StudentNo : model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    FirstName = firstName,
                    LastName = lastName,
                    StudentNo = model.Role == "Student" ? model.StudentNo : null,
                    DepartmentId = model.Role == "Student" ? 1 : (int?)null,
                    IsDeleted = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (model.Role == "Advisor") return RedirectToAction("Index", "Advisor");
                    if (model.Role == "Admin") return RedirectToAction("Index", "Admin");

                    return RedirectToAction("Index", "StudentPanel");
                }
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            ViewBag.SelectedRole = model.Role;
            return View(model);
        }

        // --- Şifre İşlemleri (Dokunulmadı) ---
        [HttpGet] public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "E-posta adresi gerekli.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                try
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action("ResetPassword", "Account",
                        new { token = token, email = email }, protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(email, "Şifre Sıfırlama",
                        $"Şifrenizi sıfırlamak için lütfen <a href='{callbackUrl}'>buraya tıklayın</a>.");

                    ViewBag.Message = "Şifre sıfırlama bağlantısı başarıyla gönderildi.";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Mail gönderilirken hata oluştu! Detay: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "Eğer bu e-posta adresi sistemde kayıtlı ise bağlantı gönderilecektir.";
            }
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null) return RedirectToAction("Index", "Home");
            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string token, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return RedirectToAction("Index", "Home");

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi! Giriş yapabilirsiniz.";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet] public IActionResult AccessDenied() => View();
    }
}