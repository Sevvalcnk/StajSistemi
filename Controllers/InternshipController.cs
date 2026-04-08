using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory; // ✅ MÜHÜR: Önbellek kütüphanesi eklendi

namespace StajSistemi.Controllers
{
    [Authorize]
    public class InternshipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMemoryCache _cache; // ✅ MÜHÜR: Hafıza motoru alanı açıldı

        public InternshipController(ApplicationDbContext context, UserManager<AppUser> userManager, IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache; // ✅ MÜHÜR: Motor içeriye (Constructor) alındı
        }

        // --- 1. LİSTELEME: Şehir ve Bölüm Zekası ile Güçlendirildi ---
        [HttpGet]
        public async Task<IActionResult> Index(int? departmentId)
        {
            // 🛠️ KRİTİK HATA DÜZELTMESİ: Metin olarak gelen ID'yi sayıya çeviriyoruz
            var userIdString = _userManager.GetUserId(User);

            // Kullanıcı verisini ve şehir bilgisini nesne olarak çekiyoruz
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
                .Where(x => !x.IsDeleted && x.Status == "Aktif");

            // 🧠 AKILLI SIRALAMA (Matching): 
            // Eğer öğrencinin şehri (user.CityId) ilanın şehriyle (x.CityId) eşleşiyorsa en üste al!
            if (user != null && user.CityId != 0)
            {
                query = query.OrderByDescending(x => x.CityId == user.CityId)
                             .ThenByDescending(x => x.CreatedDate);

                ViewBag.SpecialMessage = $"Kardaşım, {user.City?.Name} şehrindeki ve bölümüne uygun ilanları senin için önceliklendirdim! 🛡️";
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }

            // 🧠 AKILLI TAVSİYE: Eğer tavsiyeden geliniyorsa listeyi bölüme göre süz
            if (departmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == departmentId.Value);
                ViewBag.SpecialMessage = "Bölümüne en uygun staj fırsatlarını senin için mühürledim! 🥂";
            }

            var internships = await query.ToListAsync();

            // ✅ HAFTA 4 MÜHÜRÜ: Öğrencinin hangi ilanlara başvurduğunu View tarafına fısıldıyoruz
            if (user != null)
            {
                ViewBag.AppliedIds = await _context.InternshipApplications
                    .Where(a => a.AppUserId == user.Id && !a.IsDeleted)
                    .Select(a => a.InternshipId)
                    .ToListAsync();
            }

            return View(internships);
        }

        // --- 🛡️ 2. ARŞİV SİSTEMİ: Silinen İlanları Görme ve Geri Getirme ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpGet]
        public async Task<IActionResult> Archive()
        {
            var archived = await _context.Internships
                .Include(i => i.Department)
                .Include(i => i.City)
                .Where(x => x.IsDeleted == true) // Sadece silinenleri çekiyoruz
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
                internship.IsDeleted = false; // Görünmezliği kaldırıyoruz ✅
                internship.Status = "Aktif";   // Durumu düzelterek sisteme geri sokuyoruz

                _context.Update(internship);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "İlan başarıyla arşivden çıkarıldı ve sisteme geri döndü! 🥂🛡️";
            }
            return RedirectToAction(nameof(Archive));
        }

        // --- 3. YENİ İLAN OLUŞTURMA (GET) ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // 🧠 AKILLI HAFIZA (Cache): Şehirleri hafızadan alıyoruz
            if (!_cache.TryGetValue("CachedCities", out List<City> cities))
            {
                cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
                _cache.Set("CachedCities", cities, TimeSpan.FromMinutes(60));
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Cities = cities;
            return View();
        }

        // --- 3. YENİ İLAN OLUŞTURMA (POST) ---
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
                    ModelState.AddModelError("", $"❌ Hata: Seçtiğiniz tarihler arasında resmi tatiller ve hafta sonları hariç sadece {gercekIsGunu} iş günü var. Staj en az 30 iş günü olmalıdır!");
                    ViewBag.Departments = await _context.Departments.ToListAsync();
                    ViewBag.Cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
                    return View(model);
                }

                model.CreatedDate = DateTime.Now;
                model.Status = "Aktif";
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

        // --- 4. İLAN DÜZENLEME ---
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
                    TempData["SuccessMessage"] = "İlan başarıyla güncellendi! ✨";
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

        // --- 🛡️ 4. İLAN SİLME (ARŞİVE KALDIRMA) ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInternship(int id)
        {
            var internship = await _context.Internships.FindAsync(id);
            if (internship == null) return NotFound();

            internship.IsDeleted = true;
            internship.Status = "Silindi";

            _context.Update(internship);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "İlan başarıyla arşive kaldırıldı! 🥂";
            return RedirectToAction(nameof(Index));
        }

        // --- 5. BAŞVURU YAPMA (GET) ---
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var internship = await _context.Internships
                .Include(i => i.Department)
                .Include(i => i.City)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null) return NotFound();

            ViewBag.Internship = internship;

            var model = new InternshipApplication { InternshipId = id };
            return View(model);
        }

        // --- 🚀 5. BAŞVURU YAPMA (POST): AKILLI TAVSİYE, KONTENJAN VE LOGLAR ---
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int internshipId, IFormFile cvFile, IFormFile certFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var internship = await _context.Internships
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.Id == internshipId);

            if (internship == null || internship.Quota <= 0)
            {
                TempData["ErrorMessage"] = "Kontenjan dolu veya ilan kaldırılmış!";
                return RedirectToAction(nameof(Index));
            }

            if (user.DepartmentId != internship.DepartmentId)
            {
                TempData["WrongDepartment"] = true;
                TempData["UserDeptId"] = user.DepartmentId;
                TempData["ErrorMessage"] = "Bu ilan senin bölümünle ( " + internship.Department?.DepartmentName + " ) uyuşmuyor. Senin için uygun önerilerim var! 🛡️";

                return RedirectToAction("Apply", new { id = internshipId });
            }

            var alreadyApplied = await _context.InternshipApplications
                .AnyAsync(a => a.InternshipId == internshipId && a.AppUserId == user.Id && !a.IsDeleted);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] = "Bu ilana zaten başvurunuz bulunuyor!";
                return RedirectToAction(nameof(Index));
            }

            string[] allowedExtensions = { ".pdf", ".jpg", ".png", ".doc", ".docx" };
            long maxFileSize = 5 * 1024 * 1024;

            if (cvFile != null)
            {
                var ext = Path.GetExtension(cvFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext) || cvFile.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] = "CV formatı geçersiz veya 5MB'dan büyük! 🛡️";
                    return RedirectToAction("Apply", new { id = internshipId });
                }
            }
            if (certFile != null)
            {
                var ext = Path.GetExtension(certFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext) || certFile.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] = "Sertifika formatı geçersiz veya 5MB'dan büyük! 🛡️";
                    return RedirectToAction("Apply", new { id = internshipId });
                }
            }

            // Dosya Kayıt ve GUID İsimlendirme
            string cvFileName = cvFile != null ? await SaveFileAsync(cvFile, "cv") : null;
            string certFileName = certFile != null ? await SaveFileAsync(certFile, "certificates") : null;

            // ✅ HAFTA 3 & 5 MÜHÜRÜ: IP Takibi ve Dosya Loglarını Kaydet
            string userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            if (cvFileName != null)
            {
                _context.FileLogs.Add(new FileLog { AppUserId = user.Id, FileName = cvFileName, FileType = "CV", UploadTime = DateTime.Now, IpAddress = userIp });
            }
            if (certFileName != null)
            {
                _context.FileLogs.Add(new FileLog { AppUserId = user.Id, FileName = certFileName, FileType = "Sertifika", UploadTime = DateTime.Now, IpAddress = userIp });
            }

            var application = new InternshipApplication
            {
                AppUserId = user.Id,
                InternshipId = internshipId,
                CVPath = cvFileName,
                CertificatePath = certFileName,
                ApplicationDate = DateTime.Now,
                Status = "Beklemede",
                IsDeleted = false
            };

            internship.Quota -= 1; // ✅ Kontenjan Düştü!
            _context.InternshipApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Başvurunuz başarıyla mühürlendi! 🥂🔥";
            return RedirectToAction(nameof(Index));
        }

        // --- ✅ 6. BAŞVURU DURUMU GÜNCELLEME ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                TempData["ErrorMessage"] = "Hata: Durum bilgisi mühürlenemedi!";
                return RedirectToAction(nameof(Applications));
            }

            var application = await _context.InternshipApplications
                .Include(a => a.Internship).ThenInclude(i => i.City)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            if (status == "Reddedildi" && application.Status != "Reddedildi")
            {
                application.Internship.Quota += 1;
            }
            else if (status == "Onaylandı" && application.Status == "Reddedildi")
            {
                if (application.Internship.Quota > 0)
                {
                    application.Internship.Quota -= 1;
                }
                else
                {
                    TempData["ErrorMessage"] = "Kontenjan dolduğu için bu başvuru onaylanamıyor!";
                    return RedirectToAction(nameof(Applications));
                }
            }

            application.Status = status;
            _context.Update(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Başvuru durumu '{status}' olarak mühürlendi! 🥂";
            return RedirectToAction(nameof(Applications));
        }

        // --- LİSTELEME METOTLARI ---
        [Authorize(Roles = "Admin,Advisor")]
        public async Task<IActionResult> Applications()
        {
            var applications = await _context.InternshipApplications
                .Include(a => a.Internship).ThenInclude(i => i.Department)
                .Include(a => a.Internship).ThenInclude(i => i.City)
                .Include(a => a.AppUser)
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();
            return View(applications);
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

        // --- 🛠️ YARDIMCI METOTLAR: RESMİ TATİLLER VE DOSYA KAYIT ---
        private int IsGunuHesapla(DateTime baslangic, DateTime bitis, int haftalikGun)
        {
            List<DateTime> tatiller = new List<DateTime>
            {
                new DateTime(2026, 1, 1),
                new DateTime(2026, 3, 20), new DateTime(2026, 3, 21), new DateTime(2026, 3, 22),
                new DateTime(2026, 4, 23),
                new DateTime(2026, 5, 1),
                new DateTime(2026, 5, 19),
                new DateTime(2026, 5, 27), new DateTime(2026, 5, 28), new DateTime(2026, 5, 29), new DateTime(2026, 5, 30),
                new DateTime(2026, 7, 15),
                new DateTime(2026, 8, 30),
                new DateTime(2026, 10, 29)
            };

            int toplamIsGunu = 0;
            for (DateTime date = baslangic; date <= bitis; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday) continue;
                if (haftalikGun == 5 && date.DayOfWeek == DayOfWeek.Saturday) continue;
                if (tatiller.Contains(date.Date)) continue;
                toplamIsGunu++;
            }
            return toplamIsGunu;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLower();
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", subFolder);
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
            using (var stream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
    }
}