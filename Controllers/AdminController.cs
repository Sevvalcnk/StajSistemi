using AutoMapper; // 🛡️ SİBER DTO DÖNÜŞÜMÜ İÇİN ŞART
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;
using StajSistemi.DTOs;
using StajSistemi.Services; // 🚀 E-MAİL SERVİSİNE (EMAIL SENDER) ERİŞİM İÇİN ŞART
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rotativa.AspNetCore; // 🚀 PDF MOTORU İÇİN ŞART

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AdminController(UserManager<AppUser> userManager, IEmailSender emailSender, ApplicationDbContext context, IMapper mapper)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
            _mapper = mapper;
        }

        // --- 📊 1. ADMİN ANA DASHBOARD (ANALİZ VE GRAFİK MOTORU) ---
        public async Task<IActionResult> Index()
        {
            // 🛡️ SİBER DÜZELTME: Sadece 'Aktif' ve 'Silinmemiş' ilanı olan bölümleri getiriyoruz.
            var bolumVerileriRaw = await _context.Departments
                .Select(d => new {
                    BolumAdi = d.DepartmentName,
                    IlanSayisi = _context.Internships.Count(i => !i.IsDeleted &&
                                                                 i.Status == ApplicationStatus.Active &&
                                                                 i.InternshipDepartments.Any(id => id.DepartmentId == d.Id))
                })
                .ToListAsync();

            var bolumVerileri = bolumVerileriRaw.Where(x => x.IlanSayisi > 0).ToList();

            // 🛡️ SİBER FİLTRE: En çok başvuru alan ilk 5 aktif şirketi getiriyoruz.
            var popülerIlanlar = await _context.InternshipApplications
                .Include(a => a.Internship)
                .Where(a => !a.Internship.IsDeleted && a.Internship.Status == ApplicationStatus.Active)
                .GroupBy(a => a.Internship.CompanyName)
                .Select(g => new { SirketAdi = g.Key, BasvuruSayisi = g.Count() })
                .OrderByDescending(x => x.BasvuruSayisi)
                .Take(5).ToListAsync();

            // 📊 3. ANALİZ: Son 7 Günlük Başvuru Trendi
            var yediGunOnce = DateTime.Now.Date.AddDays(-7);
            var rawTrendData = await _context.InternshipApplications
                .Where(a => a.ApplicationDate >= yediGunOnce)
                .GroupBy(a => a.ApplicationDate.Date)
                .Select(g => new { TarihDate = g.Key, Sayi = g.Count() })
                .OrderBy(x => x.TarihDate).ToListAsync();

            var gunlukBasvuru = rawTrendData.Select(x => new {
                Tarih = x.TarihDate.ToString("dd/MM"),
                Sayi = x.Sayi
            }).ToList();

            // 📊 4. ANALİZ: Genel İstatistik Kartları
            var students = await _userManager.GetUsersInRoleAsync("Student");
            ViewBag.TotalStudentsCount = students.Count;
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

        // --- ➕ 2. ÖĞRENCİ EKLEME VE MAİL BİLGİLENDİRME ---
        [HttpGet] public IActionResult AddStudent() { return View(); }

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
                    <div style='font-family: Arial; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2>Merhaba {newStudent.FullName},</h2>
                        <p>Staj takip sistemine kaydınız liyakatle tamamlanmıştır.</p>
                        <p>Kullanıcı Adınız: {newStudent.UserName}</p>
                        <p>Geçici Şifreniz: <b>{temporaryPassword}</b></p>
                    </div>";

                try { await _emailSender.SendEmailAsync(newStudent.Email, subject, message); } catch { }
                return RedirectToAction("Index", "Admin");
            }
            foreach (var error in result.Errors) { ModelState.AddModelError("", error.Description); }
            return View(model);
        }

        // --- 🛡️ 🚀 3. MEZUNİYET ONAY MERKEZİ (KESİN KİLİT - SIZINTI İMHASI) ---
        [HttpGet]
        public async Task<IActionResult> GraduationQueue()
        {
            // 🛡️ SİBER MÜHÜR: 
            // 1. Sadece hocanın ONAYLADIĞI (Status == Approved) olanlar.
            // 2. Hocanın tarih mühürü bastığı (CompletedDate != null) olanlar. (SIZINTIYI BU KESER!)
            // 3. Hocanın puan verdiği (SuccessScore > 0) pırlantalar.
            // 4. ASLA silinmemiş (!IsDeleted) olanlar.

            var strictApprovedList = await _context.InternshipApplications
                .Include(a => a.AppUser)
                .Include(a => a.Internship)
                .Where(a => a.Status == ApplicationStatus.Approved) // Statü Onaylı Olmalı
                .Where(a => a.CompletedDate != null)                // Hoca Onay Tarihi OLMALI (Kesin çözüm!)
                .Where(a => a.SuccessScore > 0)                     // Puan Verilmiş Olmalı
                .Where(a => a.IsDeleted == false)                   // Arşivde Olmamalı
                .AsNoTracking()                                     // Bellekten değil, taze veriyi çek!
                .OrderByDescending(a => a.CompletedDate)
                .ToListAsync();

            return View(strictApprovedList);
        }

        // --- 🎓 📧 🚀 4. MEZUN ET, TEBRİK BELGESİ ÜRET VE MAİL GÖNDER ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeGraduation(int appId)
        {
            var application = await _context.InternshipApplications
                .Include(a => a.AppUser).ThenInclude(u => u.Department)
                .Include(a => a.Internship)
                .FirstOrDefaultAsync(a => a.Id == appId);

            if (application == null) return Json(new { success = false, message = "Kayıt bulunamadı!" });

            // 1. Statüyü "Graduated" (Mezun) Olarak Mühürle
            application.Status = ApplicationStatus.Graduated;
            _context.Update(application);
            await _context.SaveChangesAsync();

            // 🚀 2. ÖZEL MEZUNİYET BELGESİ ÜRETİMİ (TEBRİKLER BELGESİ!)
            var studentDto = _mapper.Map<StudentDto>(application.AppUser);
            studentDto.DepartmentName = application.AppUser.Department?.DepartmentName ?? "Belirtilmemiş";

            // BURASI KRİTİK: Artık defter değil, "GraduationCertificate" view'ını PDF yapıyoruz!
            var pdfResult = new ViewAsPdf("GraduationCertificate", studentDto)
            {
                FileName = $"{application.AppUser.UserName}_Mezuniyet_Sertifikasi.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                CustomSwitches = "--print-media-type"
            };

            byte[] pdfBytes = await pdfResult.BuildFile(ControllerContext);

            // 📧 3. TEBRİK MAİLİ İNŞASI
            string subject = "🎓 TEBRİKLER! Mezuniyetiniz Tescillendi ✨";
            string emailBody = $@"
                <div style='font-family: Arial; text-align: center; padding: 30px; border: 4px double #1a3a8a; border-radius: 15px;'>
                    <h1 style='color: #1a3a8a;'>🎓 MEZUNİYET TEBRİĞİ 🎓</h1>
                    <p>Sayın <strong>{application.AppUser.FullName}</strong>,</p>
                    <p>30 iş günlük staj sürecinizi başarıyla tamamlayarak mezun olmaya hak kazandınız.</p>
                    <p>Resmi <b>Staj Başarı Belgeniz</b> ekte tarafınıza sunulmuştur.</p>
                    <p>Kariyer yolculuğunuzda başarılar dileriz! ⚓️🥂</p>
                </div>";

            try
            {
                if (_emailSender is StajSistemi.Services.EmailSender customSender)
                {
                    await customSender.SendEmailWithAttachmentAsync(
                        application.AppUser.Email,
                        subject,
                        emailBody,
                        pdfBytes,
                        $"{application.AppUser.UserName}_Mezuniyet_Belgesi.pdf"
                    );
                }
                else
                {
                    await _emailSender.SendEmailAsync(application.AppUser.Email, subject, emailBody);
                }

                return Json(new { success = true, message = $"{application.AppUser.FullName} tebriklerle mezun edildi! 🎓🚀" });
            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = "Mezuniyet tescillendi ancak mail uçurulamadı: " + ex.Message });
            }
        }

        // 📜 SİBER SERTİFİKA TEMPLATE ODASI (Rotativa PDF üretirken burayı kullanır)
        [AllowAnonymous]
        public IActionResult GraduationCertificate(StudentDto model)
        {
            // Danışman ismini mühürleyelim
            ViewBag.AdvisorName = "Dr. Öğr. Hakan ÖZTÜRK";
            return View(model);
        }
    }
}