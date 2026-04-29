using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
                .Include(i => i.InternshipDepartments).ThenInclude(id => id.Department)
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

            if (departmentId.HasValue)
            {
                query = query.Where(x => x.InternshipDepartments.Any(d => d.DepartmentId == departmentId.Value));
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
                .Include(i => i.InternshipDepartments).ThenInclude(id => id.Department)
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
                .Include(i => i.InternshipDepartments).ThenInclude(id => id.Department)
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
        public async Task<IActionResult> Create(Internship model, int[] selectedDepartmentIds, int weeklyWorkDays = 5)
        {
            model.InternshipDepartments = new List<InternshipDepartment>();

            if (string.IsNullOrEmpty(model.Name))
            {
                model.Name = model.CompanyName + " Staj İlanı";
            }

            ModelState.Remove("Department");
            ModelState.Remove("AppUser");
            ModelState.Remove("Name");
            ModelState.Remove("City");
            ModelState.Remove("InternshipDepartments");

            if (ModelState.IsValid)
            {
                int gercekIsGunu = IsGunuHesapla(model.StartDate, model.EndDate, weeklyWorkDays);

                if (gercekIsGunu >= 30)
                {
                    model.CreatedDate = DateTime.Now;
                    model.Status = ApplicationStatus.Active;
                    model.IsDeleted = false;

                    _context.Internships.Add(model);
                    await _context.SaveChangesAsync();

                    if (selectedDepartmentIds != null && selectedDepartmentIds.Length > 0)
                    {
                        foreach (var deptId in selectedDepartmentIds)
                        {
                            _context.InternshipDepartments.Add(new InternshipDepartment
                            {
                                InternshipId = model.Id,
                                DepartmentId = deptId
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Yeni staj ilanı bölümleriyle birlikte başarıyla mühürlendi! 🥂";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", $"❌ Hata: Seçtiğiniz tarihler arasında sadece {gercekIsGunu} iş günü var. Staj en az 30 iş günü olmalıdır!");
            }

            ViewBag.SelectedDeptIds = selectedDepartmentIds;
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }

        // --- 🛠️ 4. İLAN DÜZENLEME ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var internship = await _context.Internships
                .Include(i => i.InternshipDepartments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null) return NotFound();

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Cities = await _context.Cities.OrderBy(x => x.Name).ToListAsync();
            return View(internship);
        }

        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Internship model, int[] selectedDepartmentIds)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Department");
            ModelState.Remove("AppUser");
            ModelState.Remove("Name");
            ModelState.Remove("City");
            ModelState.Remove("InternshipDepartments");

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Internships
                        .Include(i => i.InternshipDepartments)
                        .FirstOrDefaultAsync(i => i.Id == id);

                    if (existing == null) return NotFound();

                    existing.CompanyName = model.CompanyName;
                    existing.Description = model.Description;
                    existing.Quota = model.Quota;
                    existing.StartDate = model.StartDate;
                    existing.EndDate = model.EndDate;
                    existing.CityId = model.CityId;
                    existing.Status = model.Status;
                    existing.Name = string.IsNullOrEmpty(model.Name) ? model.CompanyName + " Staj İlanı" : model.Name;

                    _context.InternshipDepartments.RemoveRange(existing.InternshipDepartments);
                    await _context.SaveChangesAsync();

                    if (selectedDepartmentIds != null)
                    {
                        foreach (var deptId in selectedDepartmentIds)
                        {
                            _context.InternshipDepartments.Add(new InternshipDepartment
                            {
                                InternshipId = id,
                                DepartmentId = deptId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "İlan bilgileri liyakatle güncellendi! ✨";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Internships.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
            }
            ViewBag.SelectedDeptIds = selectedDepartmentIds;
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

        // --- 🚀 5. BAŞVURU MANTIĞI ---
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString)) return Challenge();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userIdString));

            var internship = await _context.Internships
                .Include(i => i.InternshipDepartments).ThenInclude(id => id.Department)
                .Include(i => i.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (internship == null) return NotFound();

            if (user != null)
            {
                TempData["UserDeptId"] = user.DepartmentId;
                TempData.Keep("UserDeptId");
            }

            return View(internship);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int internshipId, IFormFile cvFile, IFormFile certFile)
        {
            var currentUserIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserIdString)) return Challenge();

            var currentUserId = int.Parse(currentUserIdString);

            // 🛡️ SİBER DÜZELTME: Kullanıcıyı GPA verisiyle birlikte taze bir şekilde çekiyoruz
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user == null) return Challenge();

            var internship = await _context.Internships
                .Include(i => i.InternshipDepartments)
                .FirstOrDefaultAsync(i => i.Id == internshipId);

            if (internship == null || internship.Quota <= 0)
            {
                TempData["ErrorMessage"] = "Üzgünüz, kontenjan dolmuş veya ilan kaldırılmış!";
                return RedirectToAction(nameof(Index));
            }

            if (!internship.InternshipDepartments.Any(d => d.DepartmentId == user.DepartmentId))
            {
                TempData["ErrorMessage"] = "Bu ilan senin bölümünle uyuşmuyor kardaşım! 🛡️";
                return RedirectToAction("Apply", new { id = internshipId });
            }

            string[] allowedExtensions = { ".pdf", ".doc", ".docx" };
            if (cvFile != null && !allowedExtensions.Contains(Path.GetExtension(cvFile.FileName).ToLower()))
            {
                TempData["ErrorMessage"] = "❌ Geçersiz format! Sadece PDF veya Word kabul edilir.";
                return RedirectToAction("Apply", new { id = internshipId });
            }

            var alreadyApplied = await _context.InternshipApplications
                .AnyAsync(a => a.InternshipId == internshipId && a.AppUserId == user.Id && !a.IsDeleted);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] = "Bu ilana zaten aktif bir başvurunuz bulunuyor!";
                return RedirectToAction(nameof(Index));
            }

            string cvFileName = await SaveFileAsync(cvFile, "cv");
            string certFileName = certFile != null ? await SaveFileAsync(certFile, "certificates") : null;

            var application = new InternshipApplication
            {
                AppUserId = user.Id,
                InternshipId = internshipId,
                CVPath = cvFileName,
                CertificatePath = certFileName,
                ApplicationDate = DateTime.Now,
                Status = ApplicationStatus.Pending,
                IsDeleted = false,
                // ✅ KRİTİK MÜHÜR: GPA'yı 100 ile çarparak pırlanta gibi kaydediyoruz
                SuccessScore = (double)((user.GPA ?? 0) * 100)
            };

            internship.Quota -= 1;

            _context.InternshipApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Staj başvurunuz başarıyla mühürlendi! 🥂🔥";
            return RedirectToAction(nameof(Index));
        }

        // --- 6. DURUM GÜNCELLEME (ONARILAN KISIM) ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ApplicationStatus status)
        {
            var application = await _context.InternshipApplications
                .Include(a => a.Internship).ThenInclude(i => i.City)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            // ✅ SİBER NİZAM: İsimler modeldeki yeni mühürlerle (LogDate, Comment, vb.) eşitlendi!
            var statusLog = new InternshipApplicationLog
            {
                InternshipApplicationId = application.Id, // Eskisi: ApplicationId
                OldStatus = application.Status,
                NewStatus = status,
                ChangedBy = User.Identity?.Name ?? "Yetkili",
                LogDate = DateTime.Now,                   // Eskisi: ChangeDate
                Comment = "Başvuru durumu sistem üzerinden güncellendi." // Eskisi: Note
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

        // --- YARDIMCI METOTLAR ---
        [Authorize(Roles = "Admin,Advisor")]
        public async Task<IActionResult> Applications()
        {
            var apps = await _context.InternshipApplications
                .Include(a => a.Internship).ThenInclude(i => i.InternshipDepartments).ThenInclude(id => id.Department)
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
                .Include(a => a.Internship).ThenInclude(i => i.InternshipDepartments).ThenInclude(id => id.Department)
                .Include(a => a.Internship).ThenInclude(i => i.City)
                .Where(a => a.AppUserId == user.Id && !a.IsDeleted).ToListAsync();
            return View(myApps);
        }

        private int IsGunuHesapla(DateTime baslangic, DateTime bitis, int haftalikGun)
        {
            List<DateTime> tatiller = new List<DateTime> {
                new DateTime(2026, 1, 1), new DateTime(2026, 4, 23),
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
            if (file == null) return null;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLower();
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", subFolder);
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
            using (var stream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create)) { await file.CopyToAsync(stream); }
            return fileName;
        }
    }
}