using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

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

            var allAppsForTimeline = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.Internship);
            var activeAppForTimeline = allAppsForTimeline
                .OrderByDescending(a => a.ApplicationDate)
                .FirstOrDefault(a => a.AppUserId == studentDto.Id && !a.IsDeleted);

            ViewBag.ActiveAppForTimeline = activeAppForTimeline;

            // 🛡️ MÜHÜR: Profildeki skor direkt öğrencinin ortalaması (2.80 -> 280)
            double currentGpa = (double)(studentDto.GPA > 4 ? studentDto.GPA / 10.0 : studentDto.GPA);
            ViewBag.TotalScore = (int)(currentGpa * 100);

            var internshipsQuery = await _unitOfWork.Internships.GetAllIncludingAsync(i => i.Department, i => i.City);
            var allInternships = internshipsQuery.Where(i => !i.IsDeleted && i.Status == ApplicationStatus.Active);

            var appliedApps = await _unitOfWork.InternshipApplications.GetAllAsync();
            ViewBag.AppliedInternshipIds = appliedApps
                .Where(a => a.AppUserId == studentDto.Id)
                .Select(a => a.InternshipId)
                .ToList();

            if (filter == "suitable")
            {
                allInternships = allInternships.Where(i => i.DepartmentId == studentDto.DepartmentId);
                ViewBag.FilterMode = "suitable";
            }

            var smartMatches = allInternships
                .Select(ilan => new
                {
                    Ilan = ilan,
                    Score = (ilan.DepartmentId == studentDto.DepartmentId ? 50 : 0) +
                            (ilan.CityId == studentDto.CityId ? 30 : 0) +
                            (int)(currentGpa * 5) +
                            (string.IsNullOrEmpty(studentDto.PersonalSkills) ? 0 :
                                studentDto.PersonalSkills.Split(',').Count(s => ilan.Description != null && ilan.Description.Contains(s.Trim(), StringComparison.OrdinalIgnoreCase)) * 5)
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

        // --- 🛡️ GÜVENLİK KAPISI ---
        public async Task<IActionResult> Apply(int id)
        {
            var studentDto = await GetLoggedInStudentDto();
            var allApps = await _unitOfWork.InternshipApplications.GetAllAsync();
            if (allApps.Any(a => a.AppUserId == studentDto.Id && a.InternshipId == id))
            {
                TempData["AlreadyApplied"] = "True";
                return RedirectToAction(nameof(Index));
            }

            var ilan = await _unitOfWork.Internships.GetByIdAsync(id);
            if (ilan == null) return NotFound();

            if (studentDto.DepartmentId != ilan.DepartmentId)
            {
                TempData["DeptMismatch"] = "True";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // --- 🚀 BAŞVURU KAYIT METODU ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmApplication(int InternshipId, IFormFile CVFile, IFormFile? CertificateFile)
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return Unauthorized();

            var existingApps = await _unitOfWork.InternshipApplications.GetAllAsync();
            if (existingApps.Any(a => a.AppUserId == studentDto.Id && a.InternshipId == InternshipId))
            {
                TempData["AlreadyApplied"] = "True";
                return RedirectToAction(nameof(Index));
            }

            string[] allowedExtensions = { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(CVFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["FileError"] = "Akademik standart dışı format! Lütfen sadece PDF veya Word yükleyiniz.";
                return RedirectToAction("Details", new { id = InternshipId });
            }

            var application = new InternshipApplication
            {
                AppUserId = studentDto.Id,
                InternshipId = InternshipId,
                Status = ApplicationStatus.Pending,
                ApplicationDate = DateTime.Now,
                CVPath = await SaveFile(CVFile, "cvs")
            };

            if (CertificateFile != null) application.CertificatePath = await SaveFile(CertificateFile, "certs");

            await _unitOfWork.InternshipApplications.AddAsync(application);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = "Başvurunuz ve belgeleriniz asaletle sisteme kaydedilmiştir! 🥂";
            return RedirectToAction(nameof(Applications));
        }

        // --- 🚀 ✅ BAŞARI SKORUNU (SADECE GPA) HESAPLAYAN METOT ---
        public async Task<IActionResult> Applications()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship, a => a.Internship.Department, a => a.Internship.City);

            var myApplications = apps.Where(a => a.AppUserId == studentDto.Id && !a.IsDeleted).ToList();

            foreach (var app in myApplications)
            {
                // 🛡️ MÜHÜR: Ek puanlar silindi. GPA neyse skor o! (28 krizine karşı korumalı)
                double baseGpa = (double)(studentDto.GPA > 4 ? studentDto.GPA / 10.0 : studentDto.GPA);
                app.SuccessScore = (int)(baseGpa * 100);
            }

            return View(myApplications);
        }

        // --- 🏆 SONUÇLARIM EKRANI ---
        public async Task<IActionResult> Results()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship, a => a.Internship.Department, a => a.Internship.City);

            var myResults = apps.Where(a => a.AppUserId == studentDto.Id &&
                                           (a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Rejected))
                                 .ToList();

            foreach (var app in myResults)
            {
                // 🛡️ MÜHÜR: Buradaki skorlar da artık sadece ham ortalamayı gösteriyor.
                double baseGpa = (double)(studentDto.GPA > 4 ? studentDto.GPA / 10.0 : studentDto.GPA);
                app.SuccessScore = (int)(baseGpa * 100);
            }

            return View(myResults);
        }

        // --- 👤 3. PROFİL DÜZENLEME ---
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(StudentDto studentDto, IFormFile? CVFile, IFormFile? CertificateFile)
        {
            ModelState.Remove("Department");
            ModelState.Remove("DepartmentName");
            ModelState.Remove("City");

            // 🛡️ MÜHÜR: Tip uyuşmazlığı hatası giderildi (decimal 'm' kaldırıldı, double yapıldı)
            if (studentDto.GPA > 4.00)
            {
                ModelState.AddModelError("GPA", "Not ortalaması (GPA) 4.00 değerinden büyük olamaz.");
            }

            if (ModelState.IsValid)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentDto.Id);
                if (student == null) return NotFound();

                student.FirstName = studentDto.Name; student.LastName = studentDto.Surname;
                student.GPA = studentDto.GPA; student.PhoneNumber = studentDto.PhoneNumber;
                student.PersonalSkills = studentDto.PersonalSkills; student.EducationSummary = studentDto.EducationSummary;
                student.DepartmentId = studentDto.DepartmentId; student.CityId = studentDto.CityId;
                student.UniversityName = studentDto.UniversityName; student.FacultyName = studentDto.FacultyName;
                student.DegreeType = studentDto.DegreeType; student.BirthPlace = studentDto.BirthPlace;
                student.BirthDate = studentDto.BirthDate; student.AcademicYear = studentDto.AcademicYear;
                student.StudentNo = studentDto.StudentNo;

                if (CVFile != null && CVFile.Length > 0) student.CVPath = await SaveFile(CVFile, "cvs");
                if (CertificateFile != null && CertificateFile.Length > 0) student.CertificatePath = await SaveFile(CertificateFile, "certs");

                _unitOfWork.Students.Update(student);
                await _unitOfWork.SaveAsync();

                TempData["SuccessMessage"] = "Profiliniz başarıyla mühürlendi! ✨🥂";
                return RedirectToAction(nameof(Index));
            }

            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");
            var cities = await _unitOfWork.Cities.GetAllAsync();
            ViewBag.Cities = new SelectList(cities.OrderBy(x => x.Name), "Id", "Name");
            return View(studentDto);
        }

        // --- 🛡️ 4. STAJ DOSYASI (BELGELERİM) ---
        public async Task<IActionResult> Documents()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var approvedApps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship, a => a.Internship.City, a => a.Internship.Department);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDailyReport(int dayNumber, string? content, IFormFile? reportImage)
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return Unauthorized();

            string finalContent = string.IsNullOrWhiteSpace(content) ? "Bu gün için faaliyet girişi yapılmadı." : content;

            var apps = await _unitOfWork.InternshipApplications.GetAllAsync();
            var activeApp = apps.FirstOrDefault(a => a.AppUserId == studentDto.Id && a.Status == ApplicationStatus.Approved);

            if (activeApp == null)
            {
                TempData["ErrorMessage"] = "Rapor kaydedebilmek için önce onaylı bir stajınızın olması gerekmektedir.";
                return RedirectToAction(nameof(Index));
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
                    Content = finalContent,
                    CreatedDate = DateTime.Now
                };
                if (reportImage != null) newReport.ImagePath = await SaveFile(reportImage, "reports");
                await _unitOfWork.DailyReports.AddAsync(newReport);
            }
            else
            {
                existingReport.Content = finalContent;
                if (reportImage != null) existingReport.ImagePath = await SaveFile(reportImage, "reports");
                _unitOfWork.DailyReports.Update(existingReport);
            }

            await _unitOfWork.SaveAsync();
            TempData["SuccessMessage"] = $"{dayNumber}. Gün Raporu Mühürlendi! 🛡️🥂";
            return RedirectToAction(nameof(Documents));
        }

        // --- 🛡️ 6. VERİ TOPLAMA MOTORU ---
        private async Task<StudentDto> GetLoggedInStudentDto()
        {
            var currentUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserName)) return null;

            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.UserName == currentUserName || s.Email == currentUserName);

            if (student == null) return null;

            var dto = _mapper.Map<StudentDto>(student);
            dto.Id = student.Id;
            dto.DepartmentName = student.Department?.DepartmentName ?? "Bölüm Belirtilmemiş";
            dto.UniversityName = student.UniversityName; dto.FacultyName = student.FacultyName;
            dto.DegreeType = student.DegreeType; dto.BirthPlace = student.BirthPlace;
            dto.BirthDate = student.BirthDate; dto.AcademicYear = student.AcademicYear;
            dto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;

            return dto;
        }

        // --- 🚀 ✅ AKILLI DETAILS METODU ---
        public async Task<IActionResult> Details(int id)
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var allApps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship, a => a.Internship.Department, a => a.Internship.City);

            var mySpecificApp = allApps.FirstOrDefault(a => a.Id == id && a.AppUserId == studentDto.Id);

            Internship? internship = null;
            InternshipApplication? existingApp = null;

            if (mySpecificApp != null)
            {
                existingApp = mySpecificApp;
                internship = mySpecificApp.Internship;
            }
            else
            {
                var internships = await _unitOfWork.Internships.GetAllIncludingAsync(i => i.Department, i => i.City);
                internship = internships.FirstOrDefault(i => i.Id == id);

                if (internship != null)
                {
                    existingApp = allApps.FirstOrDefault(a => a.AppUserId == studentDto.Id && a.InternshipId == internship.Id);
                }
            }

            if (internship == null) return NotFound();

            ViewBag.SelectedInternship = internship;
            ViewBag.IsApplied = (existingApp != null);
            ViewBag.ApplicationDetail = existingApp;
            ViewBag.AppliedInternshipIds = allApps.Where(a => a.AppUserId == studentDto.Id).Select(a => a.InternshipId).ToList();

            return View(studentDto);
        }

        // --- 💾 DOSYA KAYDETME MOTORU ---
        private async Task<string> SaveFile(IFormFile file, string subFolder)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(fileStream); }
            return uniqueFileName;
        }

        public async Task<IActionResult> About() => View(await GetLoggedInStudentDto());
        public async Task<IActionResult> Contact() => View(await GetLoggedInStudentDto());
    }
}