using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;
using System.IO;

namespace StajSistemi.Controllers
{
    [Authorize]
    public class InternshipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public InternshipController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- 1. LİSTELEME: Tüm Aktif İlanlar ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var internships = await _context.Internships
                .Include(i => i.Department)
                .Where(x => !x.IsDeleted && x.Status == "Aktif")
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
            return View(internships);
        }

        // --- 2. YENİ İLAN OLUŞTURMA (GET) ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View();
        }

        // --- 2. YENİ İLAN OLUŞTURMA (POST) ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Internship model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                model.Name = model.CompanyName + " Staj İlanı";
            }

            ModelState.Remove("Department");
            ModelState.Remove("AppUser");
            ModelState.Remove("Name");

            if (ModelState.IsValid)
            {
                var duration = (model.EndDate - model.StartDate).TotalDays;
                if (duration < 30)
                {
                    ModelState.AddModelError("", "Staj süresi yönetmelik gereği en az 30 gün olmalıdır!");
                    ViewBag.Departments = await _context.Departments.ToListAsync();
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
            return View(model);
        }

        // --- 3. İLAN DÜZENLEME ---
        [Authorize(Roles = "Admin,Advisor")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var internship = await _context.Internships.FindAsync(id);
            if (internship == null) return NotFound();

            ViewBag.Departments = await _context.Departments.ToListAsync();
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
            return View(model);
        }

        // --- ✅ 4. BAŞVURU YAPMA (GET) - HATAYI ÇÖZEN KISIM ---
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var internship = await _context.Internships
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null) return NotFound();

            // İlan bilgisini sayfada göstermek için ViewBag kullanıyoruz
            ViewBag.Internship = internship;

            var model = new InternshipApplication { InternshipId = id };
            return View(model);
        }

        // --- 4. BAŞVURU YAPMA (POST) ---
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int internshipId, IFormFile cvFile, IFormFile certFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var internship = await _context.Internships.FindAsync(internshipId);
            if (internship == null || internship.Quota <= 0)
            {
                TempData["ErrorMessage"] = "Kontenjan dolu veya ilan kaldırılmış!";
                return RedirectToAction(nameof(Index));
            }

            var alreadyApplied = await _context.InternshipApplications
                .AnyAsync(a => a.InternshipId == internshipId && a.AppUserId == user.Id && !a.IsDeleted);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] = "Bu ilana zaten başvurunuz bulunuyor!";
                return RedirectToAction(nameof(Index));
            }

            string cvFileName = cvFile != null ? await SaveFileAsync(cvFile, "cv") : null;
            string certFileName = certFile != null ? await SaveFileAsync(certFile, "certificates") : null;

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

            internship.Quota -= 1;

            _context.InternshipApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Başvurunuz başarıyla mühürlendi! 🥂🔥";
            return RedirectToAction(nameof(Index));
        }

        // --- DİĞER LİSTELEME METOTLARI ---
        [Authorize(Roles = "Admin,Advisor")]
        public async Task<IActionResult> Applications()
        {
            var applications = await _context.InternshipApplications
                .Include(a => a.Internship).ThenInclude(i => i.Department)
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
                .Where(a => a.AppUserId == user.Id && !a.IsDeleted)
                .ToListAsync();
            return View(myApps);
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