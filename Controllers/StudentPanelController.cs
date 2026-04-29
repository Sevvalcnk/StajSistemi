using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentPanelController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentPanelController(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- 1. DASHBOARD (ANA SAYFA) ---
        public async Task<IActionResult> Index(string filter = "all")
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var allApplications = await _unitOfWork.InternshipApplications.GetAllAsync();
            var approvedCounts = allApplications
                .Where(a => a.Status == ApplicationStatus.Approved)
                .GroupBy(a => a.InternshipId)
                .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.ApprovedCounts = approvedCounts;

            var allAppsForTimeline = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.Internship);
            var activeAppForTimeline = allAppsForTimeline
                .Where(a => a.AppUserId == studentDto.Id && !a.IsDeleted)
                .OrderByDescending(a => a.Status == ApplicationStatus.Approved)
                .ThenByDescending(a => a.ApplicationDate)
                .FirstOrDefault();
            ViewBag.ActiveAppForTimeline = activeAppForTimeline;

            double currentGpa = (double)(studentDto.GPA > 4 ? studentDto.GPA / 10.0 : studentDto.GPA);
            ViewBag.TotalScore = (int)(currentGpa * 100);

            var internshipsQuery = await _unitOfWork.Internships.GetAllIncludingAsync(
                i => i.InternshipDepartments,
                i => i.City);

            var allInternships = internshipsQuery.Where(i => !i.IsDeleted && i.Status == ApplicationStatus.Active).ToList();

            foreach (var ilan in allInternships)
            {
                foreach (var deptMap in ilan.InternshipDepartments)
                {
                    if (deptMap.Department == null)
                        deptMap.Department = await _unitOfWork.Departments.GetByIdAsync(deptMap.DepartmentId);
                }
            }

            ViewBag.RecommendedInternships = allInternships
                .Where(i => i.InternshipDepartments.Any(id => id.DepartmentId == studentDto.DepartmentId))
                .ToList();

            ViewBag.AppliedInternshipIds = allAppsForTimeline
                .Where(a => a.AppUserId == studentDto.Id)
                .Select(a => a.InternshipId)
                .ToList();

            var filteredInternships = allInternships.AsEnumerable();
            if (filter == "suitable")
            {
                filteredInternships = filteredInternships.Where(i => i.InternshipDepartments.Any(id => id.DepartmentId == studentDto.DepartmentId));
                ViewBag.FilterMode = "suitable";
            }

            var smartMatches = filteredInternships
                .Select(ilan => new
                {
                    Ilan = ilan,
                    Score = (ilan.InternshipDepartments.Any(id => id.DepartmentId == studentDto.DepartmentId) ? 50 : 0) +
                            (ilan.CityId == studentDto.CityId ? 30 : 0) +
                            (int)(currentGpa * 5)
                })
                .OrderByDescending(x => x.Score)
                .Take(6).ToList();
            ViewBag.SmartMatches = smartMatches;

            var allMessages = await _unitOfWork.ChatMessages.GetAllIncludingAsync(m => m.Sender);
            ViewBag.StudentMessages = allMessages
                .Where(m => m.ReceiverId == studentDto.Id)
                .OrderByDescending(m => m.SentDate).ToList();

            return View(studentDto);
        }

        // --- 2. GÜVENLİK KAPISI (APPLY) ---
        public async Task<IActionResult> Apply(int id)
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var allApps = await _unitOfWork.InternshipApplications.GetAllAsync();

            if (allApps.Any(a => a.AppUserId == studentDto.Id && a.InternshipId == id && !a.IsDeleted))
            {
                TempData["AlreadyApplied"] = "True";
                return RedirectToAction(nameof(Index));
            }

            var ilanlar = await _unitOfWork.Internships.GetAllIncludingAsync(i => i.InternshipDepartments);
            var ilan = ilanlar.FirstOrDefault(x => x.Id == id);

            if (ilan == null) return NotFound();

            if (ilan.InternshipDepartments != null && ilan.InternshipDepartments.Any())
            {
                bool isCompatible = ilan.InternshipDepartments.Any(id => id.DepartmentId == studentDto.DepartmentId);
                if (!isCompatible)
                {
                    TempData["DeptMismatch"] = "True";
                    return RedirectToAction(nameof(Index), new { filter = "suitable" });
                }
            }

            int currentApproved = allApps.Count(a => a.InternshipId == id && a.Status == ApplicationStatus.Approved);
            if (currentApproved >= ilan.Quota)
            {
                TempData["QuotaFull"] = "True";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // --- 3. BAŞVURU KAYIT METODU (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmApplication(int InternshipId, IFormFile CVFile, IFormFile? CertificateFile)
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return Unauthorized();

            if (CVFile == null || CVFile.Length == 0)
            {
                TempData["FileError"] = "Lütfen geçerli bir CV dosyası yükleyiniz.";
                return RedirectToAction("Details", new { id = InternshipId });
            }

            string[] allowedExtensions = { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(CVFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["FileError"] = "Siber format hatası! Lütfen sadece PDF veya Word yükleyiniz.";
                return RedirectToAction("Details", new { id = InternshipId });
            }

            var application = new InternshipApplication
            {
                AppUserId = studentDto.Id,
                InternshipId = InternshipId,
                Status = ApplicationStatus.Pending,
                ApplicationDate = DateTime.Now,
                CVPath = await SaveFile(CVFile, "cvs"),
                SuccessScore = (double)(studentDto.GPA * 100)
            };

            if (CertificateFile != null)
                application.CertificatePath = await SaveFile(CertificateFile, "certs");

            await _unitOfWork.InternshipApplications.AddAsync(application);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Başvurunuz liyakatle kaydedilmiştir! 🥂";
            return RedirectToAction(nameof(Applications));
        }

        // --- 4. BAŞVURULARIM LİSTESİ ---
        public async Task<IActionResult> Applications()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship,
                a => a.Internship.InternshipDepartments,
                a => a.Internship.City,
                a => a.AppUser);

            var myApplications = apps.Where(a => a.AppUserId == studentDto.Id && !a.IsDeleted).ToList();

            foreach (var app in myApplications)
            {
                if (app.Internship?.InternshipDepartments != null)
                {
                    foreach (var deptMap in app.Internship.InternshipDepartments)
                    {
                        if (deptMap.Department == null)
                            deptMap.Department = await _unitOfWork.Departments.GetByIdAsync(deptMap.DepartmentId);
                    }
                }
            }

            return View(myApplications);
        }

        // --- 5. BAŞVURU SONUÇLARIM ---
        public async Task<IActionResult> Results()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship,
                a => a.Internship.InternshipDepartments,
                a => a.Internship.City,
                a => a.AppUser);

            var myResults = apps.Where(a => a.AppUserId == studentDto.Id &&
                                           (a.Status == ApplicationStatus.Approved ||
                                            a.Status == ApplicationStatus.Rejected ||
                                            a.Status.ToString() == "Completed")).ToList();

            foreach (var app in myResults)
            {
                if (app.Internship?.InternshipDepartments != null)
                {
                    foreach (var deptMap in app.Internship.InternshipDepartments)
                    {
                        if (deptMap.Department == null)
                            deptMap.Department = await _unitOfWork.Departments.GetByIdAsync(deptMap.DepartmentId);
                    }
                }
            }

            return View(myResults);
        }

        // --- 6. PROFİL DÜZENLEME (GET) ---
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return NotFound();

            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");

            var cities = await _unitOfWork.Cities.GetAllAsync();
            ViewBag.Cities = new SelectList(cities.OrderBy(x => x.Name), "Id", "Name");

            return View(studentDto);
        }

        // --- 7. PROFİL DÜZENLEME (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(StudentDto studentDto, IFormFile? CVFile, IFormFile? CertificateFile)
        {
            ModelState.Remove("Department");
            ModelState.Remove("DepartmentName");
            ModelState.Remove("City");

            if (studentDto.GPA > 4.00)
                ModelState.AddModelError("GPA", "Not ortalaması (GPA) 4.00 değerinden büyük olamaz.");

            if (ModelState.IsValid)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentDto.Id);
                if (student == null) return NotFound();

                student.FirstName = studentDto.Name;
                student.LastName = studentDto.Surname;
                student.GPA = studentDto.GPA;
                student.PhoneNumber = studentDto.PhoneNumber;
                student.PersonalSkills = studentDto.PersonalSkills;
                student.DepartmentId = studentDto.DepartmentId;
                student.CityId = studentDto.CityId;
                student.UniversityName = studentDto.UniversityName;
                student.StudentNo = studentDto.StudentNo;

                if (CVFile != null && CVFile.Length > 0)
                    student.CVPath = await SaveFile(CVFile, "cvs");

                if (CertificateFile != null && CertificateFile.Length > 0)
                    student.CertificatePath = await SaveFile(CertificateFile, "certs");

                _unitOfWork.Students.Update(student);
                await _unitOfWork.SaveAsync();

                TempData["SuccessMessage"] = "Profiliniz başarıyla mühürlendi! ✨🥂";
                return RedirectToAction(nameof(Index));
            }

            var depts = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(depts, "Id", "DepartmentName");
            var cities = await _unitOfWork.Cities.GetAllAsync();
            ViewBag.Cities = new SelectList(cities.OrderBy(x => x.Name), "Id", "Name");

            return View(studentDto);
        }

        // --- 8. STAJ DEFTERİ VE BELGELER ---
        public async Task<IActionResult> Documents()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var approvedApps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship, a => a.Internship.City, a => a.Internship.InternshipDepartments);

            var activeInternship = approvedApps.FirstOrDefault(a =>
                a.AppUserId == studentDto.Id &&
                a.Status == ApplicationStatus.Approved);

            if (activeInternship == null)
            {
                TempData["ErrorMessage"] = "Henüz onaylanmış bir staj başvurunuz bulunmamaktadır.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ActiveInternship = activeInternship;
            var reports = await _unitOfWork.DailyReports.GetAllAsync();
            ViewBag.DailyReports = reports.Where(r => r.AppUserId == studentDto.Id).OrderBy(r => r.DayNumber).ToList();

            return View(studentDto);
        }

        // --- 9. GÜNLÜK RAPOR KAYDETME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDailyReport(int dayNumber, string? content, IFormFile? reportImage)
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return Unauthorized();

            var apps = await _unitOfWork.InternshipApplications.GetAllAsync();
            var activeApp = apps.FirstOrDefault(a => a.AppUserId == studentDto.Id && a.Status == ApplicationStatus.Approved);

            if (activeApp == null) return RedirectToAction(nameof(Index));

            if (activeApp.StartedDate == null)
            {
                activeApp.StartedDate = DateTime.Now;
                _unitOfWork.InternshipApplications.Update(activeApp);
                await _unitOfWork.SaveAsync();
            }

            var allReports = await _unitOfWork.DailyReports.GetAllAsync();
            var existingReport = allReports.FirstOrDefault(r => r.AppUserId == studentDto.Id && r.DayNumber == dayNumber);

            if (existingReport == null)
            {
                var newReport = new DailyReport
                {
                    AppUserId = studentDto.Id,
                    InternshipApplicationId = activeApp.Id,
                    DayNumber = dayNumber,
                    Content = content ?? "Giriş yapılmadı.",
                    CreatedDate = DateTime.Now
                };
                if (reportImage != null)
                    newReport.ImagePath = await SaveFile(reportImage, "reports");

                await _unitOfWork.DailyReports.AddAsync(newReport);
            }
            else
            {
                existingReport.Content = content ?? existingReport.Content;
                if (reportImage != null)
                    existingReport.ImagePath = await SaveFile(reportImage, "reports");

                _unitOfWork.DailyReports.Update(existingReport);
            }

            await _unitOfWork.SaveAsync();
            TempData["SuccessMessage"] = $"{dayNumber}. Gün Raporu Mühürlendi! 🛡️🥂";
            return RedirectToAction(nameof(Documents));
        }

        // --- 10. İLAN DETAYLARI (NİHAİ ONARIM) ---
        public async Task<IActionResult> Details(int id)
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            // ✅ SİBER MÜHÜR: InternshipApplicationLogs eklendi, artık hata vermeyecek!
            var appsQuery = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship,
                a => a.Internship.InternshipDepartments,
                a => a.Internship.City,
                a => a.AppUser,
                a => a.InternshipApplicationLogs // 👈 Kırmızı çizginin sebebi burasıydı, artık onarıldı
            );

            var mySpecificApp = appsQuery.FirstOrDefault(a => a.InternshipId == id && a.AppUserId == studentDto.Id && !a.IsDeleted);

            Internship? internship = null;
            if (mySpecificApp != null)
            {
                internship = mySpecificApp.Internship;
            }
            else
            {
                var internships = await _unitOfWork.Internships.GetAllIncludingAsync(i => i.InternshipDepartments, i => i.City);
                internship = internships.FirstOrDefault(i => i.Id == id);
            }

            if (internship == null) return NotFound();

            foreach (var deptMap in internship.InternshipDepartments)
            {
                if (deptMap.Department == null)
                    deptMap.Department = await _unitOfWork.Departments.GetByIdAsync(deptMap.DepartmentId);
            }

            ViewBag.SelectedInternship = internship;
            ViewBag.MyApplication = mySpecificApp;
            ViewBag.IsApplied = (mySpecificApp != null);

            // Hocanın son notunu (açıklamayı) çekiyoruz
            ViewBag.AdvisorNote = mySpecificApp?.InternshipApplicationLogs?
                .OrderByDescending(l => l.LogDate)
                .FirstOrDefault()?.Comment ?? "Başvurunuz değerlendirme aşamasındadır.";

            return View(studentDto);
        }

        // --- HELPERS ---
        private async Task<StudentDto> GetLoggedInStudentDto()
        {
            var currentUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserName)) return null;

            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.UserName == currentUserName || s.Email == currentUserName);

            if (student == null) return null;

            var dto = _mapper.Map<StudentDto>(student);
            dto.Id = student.Id;
            dto.GPA = student.GPA;
            dto.DepartmentId = student.DepartmentId;
            dto.DepartmentName = student.Department?.DepartmentName ?? "Bölüm Belirtilmemiş";
            dto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;

            return dto;
        }

        private async Task<string> SaveFile(IFormFile file, string subFolder)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return uniqueFileName;
        }

        public async Task<IActionResult> About() => View(await GetLoggedInStudentDto());
        public async Task<IActionResult> Contact() => View(await GetLoggedInStudentDto());
    }
}