using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;
using System.Security.Claims;

namespace StajSistemi.Controllers
{
    public class DailyReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUser> _userManager; // 🛡️ Öğrenci bilgilerini çekmek için eklendi

        public DailyReportController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // --- 📖 ANA DEFTER GÖRÜNÜMÜ (Kapak Sayfası + 30 Günlük Rapor) ---
        // Link: /DailyReport/Index?studentId=37
        public async Task<IActionResult> Index(int? studentId)
        {
            // 1. Hedef Kullanıcıyı Belirle
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            int searchId = studentId ?? currentUser.Id;

            // 2. Öğrencinin Kimlik Bilgilerini Çek (Kapak Sayfası İçin)
            var studentInfo = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == searchId);

            // 3. Onaylanmış Staj Başvurusunu Bul (Eğer staj onaylı değilse deftere giremez)
            var activeApplication = await _context.InternshipApplications
                .FirstOrDefaultAsync(a => a.AppUserId == searchId && (a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Completed));

            if (activeApplication == null)
            {
                TempData["ErrorMessage"] = "Henüz onaylanmış bir staj kaydınız bulunamadı! 🛡️";
                return RedirectToAction("Index", "Home");
            }

            // 4. Günlük Raporları Çek
            var reports = await _context.DailyReports
                .Where(r => r.InternshipApplicationId == activeApplication.Id)
                .OrderBy(r => r.DayNumber)
                .ToListAsync();

            // 🎁 Verileri View'a Mühürle
            ViewBag.Student = studentInfo;
            ViewBag.Application = activeApplication;
            ViewBag.IsAdvisor = User.IsInRole("Advisor") || User.IsInRole("Admin");

            return View(reports);
        }

        // --- ✍️ YENİ GÜN EKLEME (GET) ---
        [HttpGet]
        public IActionResult Create(int applicationId)
        {
            ViewBag.ApplicationId = applicationId;
            return View();
        }

        // --- ✍️ YENİ GÜN EKLEME (POST - RESİM YÜKLEME DAHİL) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DailyReport model, IFormFile? imageFile)
        {
            // Resim yükleme ve mühürleme işlemi
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "reports");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                model.ImagePath = uniqueFileName;
            }

            model.CreatedDate = DateTime.Now;
            _context.DailyReports.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{model.DayNumber}. Gün raporun başarıyla mühürlendi! 🥂📸";
            return RedirectToAction(nameof(Index));
        }
    }
}