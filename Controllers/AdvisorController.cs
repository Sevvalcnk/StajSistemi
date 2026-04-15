using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using System.Linq;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Advisor,Admin")]
    public class AdvisorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdvisorController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // --- ✅ 1. ÖĞRENCİ LİSTESİ ---
        public async Task<IActionResult> Index(int? cityId, int? departmentId, double? minGpa)
        {
            var allStudents = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var allApps = await _unitOfWork.InternshipApplications.GetAllAsync();

            var studentQuery = allStudents.Where(s => s.IsDeleted == false);

            if (cityId.HasValue) studentQuery = studentQuery.Where(s => s.CityId == cityId);
            if (departmentId.HasValue) studentQuery = studentQuery.Where(s => s.DepartmentId == departmentId);
            if (minGpa.HasValue) studentQuery = studentQuery.Where(s => s.GPA >= minGpa);

            var filteredStudents = studentQuery.OrderByDescending(s => s.GPA).ToList();

            ViewBag.NewApplicationsCount = allApps.Count(a => a.Status == ApplicationStatus.Pending && !a.IsDeleted);

            var cities = await _unitOfWork.Cities.GetAllAsync();
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Cities = new SelectList(cities.OrderBy(c => c.Name), "Id", "Name", cityId);
            ViewBag.Departments = new SelectList(departments.OrderBy(d => d.DepartmentName), "Id", "DepartmentName", departmentId);

            var studentDtos = _mapper.Map<List<StudentDto>>(filteredStudents);

            foreach (var student in studentDtos)
            {
                var originalStudent = filteredStudents.First(s => s.Id == student.Id);
                student.StudentNo = !string.IsNullOrWhiteSpace(originalStudent.StudentNo) ? originalStudent.StudentNo : originalStudent.UserName;

                var lastApp = allApps
                    .Where(a => a.AppUserId == student.Id && !a.IsDeleted)
                    .OrderByDescending(a => a.ApplicationDate)
                    .FirstOrDefault();

                if (lastApp != null)
                {
                    student.InternshipStatus = lastApp.Status switch
                    {
                        ApplicationStatus.Approved => "Onaylandı",
                        ApplicationStatus.Rejected => "Reddedildi",
                        ApplicationStatus.Pending => "Beklemede",
                        _ => "İşlemde"
                    };
                }
                else { student.InternshipStatus = "Başvuru Yok"; }
            }

            return View(studentDtos);
        }

        // --- ✅ 2. ÖĞRENCİ DETAYI ---
        public async Task<IActionResult> StudentDetails(int id)
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == id);

            if (student == null || student.IsDeleted)
            {
                TempData["ErrorMessage"] = "Öğrenci bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;

            return View(studentDto);
        }

        // --- 🛡️ 3. ÖĞRENCİ STAJ DOSYASI İZLEME (31 SAYFA GARANTİLİ) ---
        // Image 2'deki Kapak Bilgilerinin hocada görünmesini sağlayan motor burasıdır.
        public async Task<IActionResult> ViewStudentFile(int studentId)
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == studentId);

            if (student == null) return NotFound();

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;
            studentDto.DepartmentName = student.Department?.DepartmentName ?? "Bölüm Belirtilmemiş";

            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship, a => a.Internship.City, a => a.Internship.Department);

            // GÜVENLİK: Sadece bölümüyle eşleşen onaylı staj (Hatalı kurum ismini bitirir)
            var activeApp = apps.FirstOrDefault(a => a.AppUserId == studentId && a.Status == ApplicationStatus.Approved && a.Internship.DepartmentId == student.DepartmentId);

            ViewBag.ActiveInternship = activeApp;

            var reports = await _unitOfWork.DailyReports.GetAllAsync();
            var studentReports = reports.Where(r => r.AppUserId == studentId).OrderBy(r => r.DayNumber).ToList();

            ViewBag.DailyReports = studentReports;

            return View("../StudentPanel/Documents", studentDto);
        }

        // --- 🛡️ 🚀 YENİ MÜHÜR: ÖĞRENCİ DASHBOARD İZLEME (ÇİZELGE GARANTİLİ) ---
        // Image 1'deki "Takip Çizelgesi"nin hocada görünmesini sağlayan asil metot budur.
        public async Task<IActionResult> ViewStudentDashboard(int studentId)
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == studentId);
            if (student == null) return NotFound();

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;
            studentDto.DepartmentName = student.Department?.DepartmentName ?? "Bölüm Belirtilmemiş";

            // 🛡️ Çizelge için aktif başvuruyu çekiyoruz
            var allApps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.Internship);
            var activeAppForTimeline = allApps
                .Where(a => a.AppUserId == studentId && !a.IsDeleted)
                .OrderByDescending(a => a.ApplicationDate)
                .FirstOrDefault();

            ViewBag.ActiveAppForTimeline = activeAppForTimeline; // Dashboard'daki çizelgeyi hoca için canlandırır.
            ViewBag.AppliedInternshipIds = allApps.Where(a => a.AppUserId == studentId).Select(a => a.InternshipId).ToList();

            // Puanlama ve akıllı eşleşmeler (Hoca her şeyi görmeli)
            ViewBag.TotalScore = 350;

            return View("../StudentPanel/Index", studentDto);
        }

        // --- ✅ 4. SOFT DELETE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student != null)
            {
                student.IsDeleted = true;
                _unitOfWork.Students.Update(student);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "Öğrenci kaydı arşive kaldırıldı. 🥂";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- ✅ 5. STAJ BAŞVURULARI LİSTESİ ---
        public async Task<IActionResult> InternshipApplications()
        {
            var applications = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.AppUser, a => a.Internship);

            var activeApplications = applications
                .Where(a => a.IsDeleted == false)
                .OrderByDescending(a => a.SuccessScore)
                .ThenByDescending(a => a.ApplicationDate)
                .ToList();

            ViewBag.NewApplicationsCount = activeApplications.Count(a => a.Status == ApplicationStatus.Pending);
            return View(activeApplications);
        }

        // --- 🔥 6. BAŞVURU DURUMUNU GÜNCELLE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, ApplicationStatus status)
        {
            var applications = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.Internship);
            var app = applications.FirstOrDefault(a => a.Id == id);

            if (app == null) return NotFound();
            app.Status = status;

            _unitOfWork.InternshipApplications.Update(app);
            await _unitOfWork.SaveAsync();

            string mesaj = status == ApplicationStatus.Approved ? "Onaylandı" : "Reddedildi";
            TempData["SuccessMessage"] = $"Başvuru başarıyla '{mesaj}' olarak mühürlendi! ✨";

            return RedirectToAction(nameof(InternshipApplications));
        }
    }
}