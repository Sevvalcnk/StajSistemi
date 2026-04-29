using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<AppUser> userManager, IEmailSender emailSender, ApplicationDbContext context)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 📊 1. ANALİZ: Bölüm Bazlı İlan Dağılımı (SADELEŞTİRİLDİ!)
            // 🛡️ SİBER DÜZELTME: Sadece 'Aktif' ve 'Silinmemiş' ilanı olan bölümleri getiriyoruz.
            var bolumVerileriRaw = await _context.Departments
                .Select(d => new {
                    BolumAdi = d.DepartmentName,
                    IlanSayisi = _context.Internships.Count(i => !i.IsDeleted &&
                                                                 i.Status == ApplicationStatus.Active &&
                                                                 i.InternshipDepartments.Any(id => id.DepartmentId == d.Id))
                })
                .ToListAsync();

            // Sadece IlanSayisi 0'dan büyük olanları filtreleyerek o karışık grafiği pırlanta gibi yapıyoruz.
            var bolumVerileri = bolumVerileriRaw.Where(x => x.IlanSayisi > 0).ToList();

            // 📊 2. ANALİZ: En Çok Başvuru Alan Şirketler (HAYALET VERİ TEMİZLİĞİ!)
            // 🛡️ SİBER FİLTRE: Sadece yayında olan (Silinmemiş ve Aktif) ilanların başvurularını sayıyoruz.
            // Bu sayede "Technode" gibi hayalet şirketler grafikten temizlenir.
            var popülerIlanlar = await _context.InternshipApplications
                .Include(a => a.Internship)
                .Where(a => !a.Internship.IsDeleted && a.Internship.Status == ApplicationStatus.Active)
                .GroupBy(a => a.Internship.CompanyName)
                .Select(g => new {
                    SirketAdi = g.Key,
                    BasvuruSayisi = g.Count()
                })
                .OrderByDescending(x => x.BasvuruSayisi)
                .Take(5).ToListAsync();

            // 📊 3. ANALİZ: Son 7 Günlük Başvuru Trendi
            var yediGunOnce = DateTime.Now.Date.AddDays(-7);
            var rawTrendData = await _context.InternshipApplications
                .Where(a => a.ApplicationDate >= yediGunOnce)
                .GroupBy(a => a.ApplicationDate.Date)
                .Select(g => new {
                    TarihDate = g.Key,
                    Sayi = g.Count()
                })
                .OrderBy(x => x.TarihDate)
                .ToListAsync();

            var gunlukBasvuru = rawTrendData.Select(x => new {
                Tarih = x.TarihDate.ToString("dd/MM"),
                Sayi = x.Sayi
            }).ToList();

            // 📊 4. ANALİZ: Genel İstatistik Kartları
            var students = await _userManager.GetUsersInRoleAsync("Student");
            ViewBag.TotalStudentsCount = students.Count;

            // Kartlardaki sayıyı da aktif ilanlara göre güncelliyoruz.
            ViewBag.ActiveInternships = await _context.Internships.CountAsync(x => !x.IsDeleted && x.Status == ApplicationStatus.Active);
            ViewBag.PendingApplications = await _context.InternshipApplications.CountAsync(a => a.Status == ApplicationStatus.Pending);

            // 🛡️ VERİLERİ VİEW'A FIRLATMA
            ViewBag.BolumLabels = bolumVerileri.Select(x => x.BolumAdi).ToArray();
            ViewBag.BolumCounts = bolumVerileri.Select(x => x.IlanSayisi).ToArray();

            ViewBag.CompanyLabels = popülerIlanlar.Select(x => x.SirketAdi).ToArray();
            ViewBag.AppCounts = popülerIlanlar.Select(x => x.BasvuruSayisi).ToArray();

            ViewBag.TrendLabels = gunlukBasvuru.Select(x => x.Tarih).ToArray();
            ViewBag.TrendCounts = gunlukBasvuru.Select(x => x.Sayi).ToArray();

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
            string rawGuid = Guid.NewGuid().ToString().Replace("-", "");
            string temporaryPassword = "Staj" + rawGuid.Substring(0, 5).ToUpper() + "1!";

            var newStudent = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newStudent, temporaryPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newStudent, "Student");

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
                    TempData["SuccessMessage"] = $"{newStudent.FullName} başarıyla eklendi. Şifre: {temporaryPassword}";
                }
                catch (Exception)
                {
                    TempData["WarningMessage"] = $"Öğrenci eklendi fakat mail gönderilemedi. Şifre: {temporaryPassword}";
                }

                return RedirectToAction("Index", "Admin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}