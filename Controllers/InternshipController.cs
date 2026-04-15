using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace StajSistemi.Controllers
{
    [Authorize]
    public class InternshipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMemoryCache _cache;

        public InternshipController(ApplicationDbContext context, UserManager<AppUser> userManager, IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        // --- 1. LİSTELEME: Akıllı Sıralama ve Akıllı Filtre Kapısı ---
        [HttpGet]
        public async Task<IActionResult> Index(int? departmentId)
        {
            var userIdString = _userManager.GetUserId(User);

            AppUser user = null;
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                user = await _userManager.Users
                    .Include(u => u.City)
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }

            var query = _context.Internships
                .Include(i => i.Department)
                .Include(i => i.City)
                .Where(x => !x.IsDeleted && x.Status == ApplicationStatus.Active);

            if (user != null && user.CityId != 0)
            {
                query = query.OrderByDescending(x => x.CityId == user.CityId)
                             .ThenByDescending(x => x.CreatedDate);

                ViewBag.SpecialMessage = $"Kardaşım, {user.City?.Name} şehrindeki ilanları senin için mühürledim! 🛡️";
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }

            // ✅ MÜHÜR: Gelen departmentId varsa listeyi sadece o bölüme göre süzer.
            if (departmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
                ViewBag.SpecialMessage = "Bölümüne en uygun staj fırsatlarını senin için süzdüm! 🥂";
            }

            var internships = await query.ToListAsync();

            if (user != null)
            {
                ViewBag.AppliedIds = await _context.InternshipApplications
                    .Where(a => a.AppUserId == user.Id && !a.IsDeleted)
                    .Select(a => a.InternshipId)
                    .ToListAsync();
            }

            return View(internships);
        }

        // --- 🔍 İLAN DETAY SAYFASI ---
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var internship = await _context.Internships
                .Include(i => i.Department)
                .Include(i => i.City)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null) return NotFound();

            return View(internship);
        }

        // --- 🛡️ 2. ARŞİV SİSTEMİ ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpGet]
        public async Task<IActionResult> Archive()
        {
            var archived = await _context.Internships
                .Include(i => i.Department)
                .Include(i => i.City)
                .Where(x => x.IsDeleted == true)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(archived);
        }

        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var internship = await _context.Internships.FindAsync(id);
            if (internship != null)
            {
                internship.IsDeleted = false;
                internship.Status = ApplicationStatus.Active;

                _context.Update(internship);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "İlan başarıyla arşivden çıkarıldı ve tekrar yayına mühürlendi! 🥂🛡️";
            }
            return RedirectToAction(nameof(Archive));
        }

        // --- 3. YENİ İLAN OLUŞTURMA ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!_cache.TryGetValue("CachedCities", out List<City> cities))
            {
                cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
                _cache.Set("CachedCities", cities, TimeSpan.FromMinutes(60));
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Cities = cities;
            return View();
        }

        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Internship model, int weeklyWorkDays = 5)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                model.Name = model.CompanyName + " Staj İlanı";
            }

            ModelState.Remove("Department");
            ModelState.Remove("AppUser");
            ModelState.Remove("Name");
            ModelState.Remove("City");

            if (ModelState.IsValid)
            {
                int gercekIsGunu = IsGunuHesapla(model.StartDate, model.EndDate, weeklyWorkDays);

                if (gercekIsGunu < 30)
                {
                    ModelState.AddModelError("", $"❌ Hata: Seçtiğiniz tarihler arasında sadece {gercekIsGunu} iş günü var. Staj en az 30 iş günü olmalıdır!");
                    ViewBag.Departments = await _context.Departments.ToListAsync();
                    ViewBag.Cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
                    return View(model);
                }

                model.CreatedDate = DateTime.Now;
                model.Status = ApplicationStatus.Active;
                model.IsDeleted = false;

                _context.Internships.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Yeni staj ilanı başarıyla mühürlendi! 🥂";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }

        // --- 4. İLAN DÜZENLEME VE SİLME ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var internship = await _context.Internships.FindAsync(id);
            if (internship == null) return NotFound();

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
            return View(internship);
        }

        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Internship model)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Department");
            ModelState.Remove("AppUser");
            ModelState.Remove("Name");
            ModelState.Remove("City");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "İlan bilgileri başarıyla güncellendi! ✨";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Internships.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
            }
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInternship(int id)
        {
            var internship = await _context.Internships.FindAsync(id);
            if (internship == null) return NotFound();

            internship.IsDeleted = true;
            internship.Status = ApplicationStatus.Deleted;

            _context.Update(internship);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "İlan başarıyla arşive kaldırıldı! 🥂";
            return RedirectToAction(nameof(Index));
        }

        // --- 🚀 5. BAŞVURU SAYFASINI GÖSTER (GET) ---
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userIdString));

            var internship = await _context.Internships
                .Include(i => i.Department)
                .Include(i => i.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (internship == null) return NotFound();

            // ✅ KRİTİK MÜHÜR: Yusuf'un (ve diğerlerinin) "Hatalı ID" sorununu burada bitiriyoruz.
            // Sayfa her yüklendiğinde, o anki kullanıcının GERÇEK bölüm ID'sini TempData'ya taze olarak basıyoruz.
            if (user != null)
            {
                TempData["UserDeptId"] = user.DepartmentId;
                TempData.Keep("UserDeptId");
            }

            // Ghost Warning Fix: Eğer öğrenci doğru ilana girmişse, eski hataları kafasından siliyoruz.
            if (user != null && user.DepartmentId == internship.DepartmentId)
            {
                TempData["WrongDepartment"] = null;
                TempData["ErrorMessage"] = null;
            }

            return View(internship);
        }

        // --- 🚀 5. BAŞVURU YAPMA (POST) ---
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int internshipId, IFormFile cvFile, IFormFile certFile)
        {
            var currentUserId = int.Parse(_userManager.GetUserId(User));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null) return Challenge();

            var internship = await _context.Internships
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.Id == internshipId);

            if (internship == null || internship.Quota <= 0)
            {
                TempData["ErrorMessage"] = "Üzgünüz, kontenjan dolmuş veya ilan kaldırılmış!";
                return RedirectToAction(nameof(Index));
            }

            // ✅ AKILLI BÖLÜM KONTROLÜ (Hata anında Yusuf'un taze ID'sini hafızada tut)
            if (user.DepartmentId != internship.DepartmentId)
            {
                TempData["WrongDepartment"] = true;
                TempData["UserDeptId"] = user.DepartmentId; // 🛡️ BU SATIR HAYATİ!
                TempData["ErrorMessage"] = "Bu ilan senin bölümünle uyuşmuyor kardaşım! 🛡️";
                return RedirectToAction("Apply", new { id = internshipId });
            }

            // ✅ GÜVENLİK RADARI: Dosya Uzantı Kontrolü
            string[] allowedExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };

            if (cvFile != null && !allowedExtensions.Contains(Path.GetExtension(cvFile.FileName).ToLower()))
            {
                TempData["ErrorMessage"] = "❌ Geçersiz format! Özgeçmiş için sadece PDF veya Word kabul edilir.";
                return RedirectToAction("Apply", new { id = internshipId });
            }

            if (certFile != null && !allowedExtensions.Contains(Path.GetExtension(certFile.FileName).ToLower()))
            {
                TempData["ErrorMessage"] = "❌ Geçersiz format! Sertifika için sadece PDF, Word veya PPT kabul edilir.";
                return RedirectToAction("Apply", new { id = internshipId });
            }

            var alreadyApplied = await _context.InternshipApplications
                .AnyAsync(a => a.InternshipId == internshipId && a.AppUserId == user.Id && !a.IsDeleted);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] = "Bu ilana zaten aktif bir başvurunuz bulunuyor!";
                return RedirectToAction(nameof(Index));
            }

            string cvFileName = cvFile != null ? await SaveFileAsync(cvFile, "cv") : null;
            string certFileName = certFile != null ? await SaveFileAsync(certFile, "certificates") : null;

            string userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (cvFileName != null) _context.FileLogs.Add(new FileLog { AppUserId = user.Id, FileName = cvFileName, FileType = "Özgeçmiş", UploadTime = DateTime.Now, IpAddress = userIp });
            if (certFileName != null) _context.FileLogs.Add(new FileLog { AppUserId = user.Id, FileName = certFileName, FileType = "Sertifika", UploadTime = DateTime.Now, IpAddress = userIp });

            // ✅ BAŞARI SKORU HESAPLAMA MÜHÜRÜ
            var application = new InternshipApplication
            {
                AppUserId = user.Id,
                InternshipId = internshipId,
                CVPath = cvFileName,
                CertificatePath = certFileName,
                ApplicationDate = DateTime.Now,
                Status = ApplicationStatus.Pending,
                IsDeleted = false,
                SuccessScore = (user.GPA ?? 0) * 100
            };

            internship.Quota -= 1;
            _context.InternshipApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Staj başvurunuz başarıyla mühürlendi! Başarılar dileriz. 🥂🔥";
            return RedirectToAction(nameof(Index));
        }

        // --- ✅ 6. DURUM GÜNCELLEME (Kara Kutu Loglama ve Kontenjan Yönetimi) ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ApplicationStatus status)
        {
            var application = await _context.InternshipApplications
                .Include(a => a.Internship).ThenInclude(i => i.City)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            var statusLog = new InternshipApplicationLog
            {
                ApplicationId = application.Id,
                OldStatus = application.Status,
                NewStatus = status,
                ChangedBy = User.Identity?.Name ?? "Yetkili",
                ChangeDate = DateTime.Now,
                Note = "Başvuru durumu sistem üzerinden güncellendi."
            };

            if (status == ApplicationStatus.Rejected && application.Status != ApplicationStatus.Rejected)
                application.Internship.Quota += 1;
            else if (status == ApplicationStatus.Approved && application.Status == ApplicationStatus.Rejected)
                application.Internship.Quota -= 1;

            application.Status = status;
            _context.InternshipApplicationLogs.Add(statusLog);

            _context.Update(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Öğrenci başvuru durumu başarıyla güncellendi ve mühürlendi! 🥂";
            return RedirectToAction(nameof(Applications));
        }

        // --- LİSTELEME VE YARDIMCI METOTLAR ---
        [Authorize(Roles = "Admin,Advisor")]
        public async Task<IActionResult> Applications()
        {
            var apps = await _context.InternshipApplications
                .Include(a => a.Internship).ThenInclude(i => i.Department)
                .Include(a => a.Internship).ThenInclude(i => i.City)
                .Include(a => a.AppUser)
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.SuccessScore)
                .ToListAsync();
            return View(apps);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            var myApps = await _context.InternshipApplications
                .Include(a => a.Internship).ThenInclude(i => i.Department)
                .Include(a => a.Internship).ThenInclude(i => i.City)
                .Where(a => a.AppUserId == user.Id && !a.IsDeleted)
                .ToListAsync();
            return View(myApps);
        }

        private int IsGunuHesapla(DateTime baslangic, DateTime bitis, int haftalikGun)
        {
            List<DateTime> tatiller = new List<DateTime> {
                new DateTime(2026, 1, 1), new DateTime(2026, 3, 20), new DateTime(2026, 4, 23),
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 19), new DateTime(2026, 7, 15),
                new DateTime(2026, 8, 30), new DateTime(2026, 10, 29)
            };
            int toplamIsGunu = 0;
            for (DateTime date = baslangic; date <= bitis; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday) continue;
                if (haftalikGun == 5 && date.DayOfWeek == DayOfWeek.Saturday) continue;
                if (tatiller.Any(t => t.Date == date.Date)) continue;
                toplamIsGunu++;
            }
            return toplamIsGunu;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLower();
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", subFolder);
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
            using (var stream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create)) { await file.CopyToAsync(stream); }
            return fileName;
        }
    }
}