using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // --- ✅ 1. ÖĞRENCİ LİSTESİ (AKTİF) ---
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

        // --- 🛡️ 🚀 YENİ MÜHÜR: EĞİTİM DÜZEYİNE GÖRE BÖLÜMLERİ GETİR ---
        [HttpGet]
        public async Task<JsonResult> GetDepartmentsByLevel(string level)
        {
            var allDepartments = await _unitOfWork.Departments.GetAllAsync();
            var filtered = allDepartments
                .Where(d => d.DepartmentName.Contains(level) || level == "Hepsi")
                .OrderBy(d => d.DepartmentName)
                .Select(d => new { id = d.Id, name = d.DepartmentName })
                .ToList();

            return Json(filtered);
        }

        // --- 🗄️ 🚀 YENİ MÜHÜR: ARŞİVLENMİŞ ÖĞRENCİLER ---
        public async Task<IActionResult> Archive(int? departmentId)
        {
            var allStudents = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var archivedQuery = allStudents.Where(s => s.IsDeleted == true);

            if (departmentId.HasValue) archivedQuery = archivedQuery.Where(s => s.DepartmentId == departmentId);

            var archivedStudents = archivedQuery.OrderByDescending(s => s.FullName).ToList();
            var studentDtos = _mapper.Map<List<StudentDto>>(archivedStudents);

            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments.OrderBy(d => d.DepartmentName), "Id", "DepartmentName", departmentId);

            TempData["InfoMessage"] = "Şu an Siber Arşiv odasındasınız. 🗄️";
            return View(studentDtos);
        }

        // --- 🔄 🚀 YENİ MÜHÜR: ÖĞRENCİYİ GERİ GETİR ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student != null)
            {
                student.IsDeleted = false;
                _unitOfWork.Students.Update(student);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = $"{student.FullName} asaletle ana listeye geri döndü! 🥂✨";
            }
            return RedirectToAction(nameof(Archive));
        }

        // --- ✅ 2. ÖĞRENCİ DETAYI ---
        public async Task<IActionResult> StudentDetails(int id)
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Öğrenci bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;

            return View(studentDto);
        }

        // --- 🛡️ 3. ÖĞRENCİ STAJ DOSYASI İZLEME ---
        public async Task<IActionResult> ViewStudentFile(int studentId)
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == studentId);

            if (student == null) return NotFound();

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;
            studentDto.DepartmentName = student.Department?.DepartmentName ?? "Bölüm Belirtilmemiş";

            // ✅ GÜNCELLEME: Many-to-Many köprüsü üzerinden bölüm kontrolü
            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship, a => a.Internship.City, a => a.Internship.InternshipDepartments);

            // SİBER DÜZELTME: Artık DepartmentId'ye değil, InternshipDepartments içindeki ID'lere bakıyoruz
            var activeApp = apps.FirstOrDefault(a =>
                a.AppUserId == studentId &&
                a.Status == ApplicationStatus.Approved &&
                a.Internship.InternshipDepartments.Any(id => id.DepartmentId == student.DepartmentId));

            ViewBag.ActiveInternship = activeApp;

            var reports = await _unitOfWork.DailyReports.GetAllAsync();
            var studentReports = reports.Where(r => r.AppUserId == studentId).OrderBy(r => r.DayNumber).ToList();

            ViewBag.DailyReports = studentReports;

            return View("../StudentPanel/Documents", studentDto);
        }

        // --- 🛡️ 🚀 YENİ MÜHÜR: ÖĞRENCİ DASHBOARD İZLEME ---
        public async Task<IActionResult> ViewStudentDashboard(int studentId)
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == studentId);
            if (student == null) return NotFound();

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;
            studentDto.DepartmentName = student.Department?.DepartmentName ?? "Bölüm Belirtilmemiş";

            var allApps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.Internship);
            var activeAppForTimeline = allApps
                .Where(a => a.AppUserId == studentId && !a.IsDeleted)
                .OrderByDescending(a => a.Status == ApplicationStatus.Approved)
                .ThenByDescending(a => a.ApplicationDate)
                .FirstOrDefault();

            ViewBag.ActiveAppForTimeline = activeAppForTimeline;
            ViewBag.AppliedInternshipIds = allApps.Where(a => a.AppUserId == studentId).Select(a => a.InternshipId).ToList();

            ViewBag.TotalScore = (int)(student.GPA * 100);

            return View("../StudentPanel/Index", studentDto);
        }

        // --- ✅ 4. SOFT DELETE (ARŞİVE KALDIR) ---
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
            var app = await _unitOfWork.InternshipApplications.GetByIdAsync(id);
            if (app == null) return NotFound();

            app.Status = status;

            if (status == ApplicationStatus.Approved)
            {
                if (app.ApprovedDate == null) app.ApprovedDate = DateTime.Now;
            }
            else if (status == ApplicationStatus.Pending)
            {
                app.ApprovedDate = null;
                app.StartedDate = null;
                app.CompletedDate = null;
            }
            else if (status == ApplicationStatus.Rejected)
            {
                app.ApprovedDate = null;
            }
            else if (status.ToString() == "Completed" || status.ToString() == "Finished")
            {
                app.CompletedDate = DateTime.Now;
            }

            _unitOfWork.InternshipApplications.Update(app);
            await _unitOfWork.SaveAsync();

            string mesaj = status == ApplicationStatus.Approved ? "Onaylandı" : "İşlem Güncellendi";
            TempData["SuccessMessage"] = $"Başvuru başarıyla '{mesaj}' olarak mühürlendi! ✨🥂";

            return RedirectToAction(nameof(InternshipApplications));
        }
    }
}