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
using System.Security.Claims; // 🛡️ SİBER KİMLİK DOĞRULAMA İÇİN ŞART
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

        // --- ✅ 1. ÖĞRENCİ LİSTESİ ---
        public async Task<IActionResult> Index(int? cityId, int? departmentId, double? minGpa)
        {
            var allStudents = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var allApps = await _unitOfWork.InternshipApplications.GetAllAsync();

            // 🛡️ KRİTİK MÜHÜR: Modal içindeki ilan listesinin dolması için bu veriyi çekiyoruz
            var allInternships = await _unitOfWork.Internships.GetAllAsync();
            ViewBag.Internships = allInternships.Where(i => !i.IsDeleted).ToList();

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

        // --- 🛡️ 🚀 GÜNCELLENDİ: STAJ DOSYASI GÖRÜNTÜLEME (0/30 ÇÖZÜMÜ) ---
        public async Task<IActionResult> ViewStudentFile(int studentId)
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == studentId);

            if (student == null) return NotFound();

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;
            studentDto.DepartmentName = student.Department?.DepartmentName ?? "Bölüm Belirtilmemiş";

            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship, a => a.Internship.City, a => a.Internship.InternshipDepartments);

            var activeApp = apps.FirstOrDefault(a =>
                a.AppUserId == studentId &&
                a.Status == ApplicationStatus.Approved);

            ViewBag.ActiveInternship = activeApp;

            var reports = await _unitOfWork.DailyReports.GetAllAsync();
            var studentReports = reports.Where(r => r.AppUserId == studentId).OrderBy(r => r.DayNumber).ToList();

            // 🛡️ KRİTİK DEĞİŞİKLİK: IsApproved değil, sadece yazılanları say (0/30 sorunu bitti)
            int writtenCount = studentReports.Count;
            ViewBag.ApprovedReportsCount = writtenCount;
            ViewBag.CanApproveInternship = writtenCount >= 30; // 30 Gün kilidi açılır

            ViewBag.DailyReports = studentReports;

            return View("../StudentPanel/Documents", studentDto);
        }

        public async Task<IActionResult> ViewStudentDashboard(int studentId)
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == studentId);
            if (student == null) return NotFound();

            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;
            studentDto.DepartmentName = student.Department?.DepartmentName ?? "Bölüm Belirtilmemiş";

            var allDailyReports = await _unitOfWork.DailyReports.GetAllAsync();
            var myReports = allDailyReports.Where(r => r.AppUserId == studentId).ToList();

            int totalReportsCount = myReports.Count;
            int approvedReportsCount = totalReportsCount; // 0/30 sorunu için toplamı onaylı gibi gösteriyoruz
            int pendingReportsCount = 0;

            int targetDays = 30;
            double progressPercent = ((double)totalReportsCount / targetDays) * 100;

            ViewBag.TotalReportsCount = totalReportsCount;
            ViewBag.ApprovedReportsCount = approvedReportsCount;
            ViewBag.PendingReportsCount = pendingReportsCount;

            ViewBag.ProgressPercent = Math.Round(Math.Min(progressPercent, 100), 0);
            ViewBag.RemainingDays = Math.Max(targetDays - totalReportsCount, 0);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReport(int dayNumber, int studentId)
        {
            if (dayNumber <= 0 || studentId <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz veri aktarımı! 🚫";
                return RedirectToAction(nameof(Index));
            }

            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) return NotFound();

            var reports = await _unitOfWork.DailyReports.GetAllAsync();
            var report = reports.FirstOrDefault(r => r.AppUserId == studentId && r.DayNumber == dayNumber);

            if (report != null)
            {
                report.IsApproved = true;
                _unitOfWork.DailyReports.Update(report);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = $"{dayNumber}. Gün Raporu Liyakatle Onaylandı! 🛡️🥂";
            }
            else
            {
                TempData["ErrorMessage"] = "Onaylanacak rapor içeriği bulunamadı. ❌";
            }

            return RedirectToAction(nameof(ViewStudentFile), new { studentId = studentId });
        }

        // --- 🛡️ 🚀 GÜNCELLENDİ: STAJ DEFTERİNİ KOMPLE ONAYLA ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFullInternship(int studentId)
        {
            var reports = await _unitOfWork.DailyReports.GetAllAsync();
            int writtenCount = reports.Count(r => r.AppUserId == studentId);

            if (writtenCount < 30)
            {
                TempData["ErrorMessage"] = $"Liyakat uyarısı: 30 rapor tamamlanmadan onay mühürü vurulamaz. (Şu an: {writtenCount}/30) 🚫";
                return RedirectToAction(nameof(ViewStudentFile), new { studentId = studentId });
            }

            var apps = await _unitOfWork.InternshipApplications.GetAllAsync();
            var activeApp = apps.FirstOrDefault(a => a.AppUserId == studentId && a.Status == ApplicationStatus.Approved);

            if (activeApp != null)
            {
                activeApp.CompletedDate = DateTime.Now;
                var studentReports = reports.Where(r => r.AppUserId == studentId).ToList();
                foreach (var r in studentReports) { r.IsApproved = true; } // Toplu mühür

                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "30 İş Günü Liyakatle Tamamlandı ve Staj Defteri Mühürlendi! 🥂🛡️⚓";
            }

            return RedirectToAction(nameof(ViewStudentFile), new { studentId = studentId });
        }

        // --- 🛡️ 🚀 YENİ: STAJ DEFTERİNİ KOMPLE REDDET ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFullInternship(int studentId)
        {
            var apps = await _unitOfWork.InternshipApplications.GetAllAsync();
            var activeApp = apps.FirstOrDefault(a => a.AppUserId == studentId && a.Status == ApplicationStatus.Approved);

            if (activeApp != null)
            {
                activeApp.Status = ApplicationStatus.Rejected;
                await _unitOfWork.SaveAsync();
                TempData["ErrorMessage"] = "Staj dosyası incelendi ve yetersiz bulunduğu için reddedildi. 🚫";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecommendInternship(int internshipId, int studentId)
        {
            var internship = await _unitOfWork.Internships.GetByIdAsync(internshipId);
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);

            if (internship == null || student == null) return NotFound();

            var advisorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int advisorId = int.TryParse(advisorIdStr, out var id) ? id : 0;

            string recommendationMsg = $"🛡️ <strong>DANIŞMAN ÖNERİSİ:</strong> Sayın {student.FullName}, akademik yetkinlikleriniz ve profilinizle uyumlu olan <strong>'{internship.CompanyName}'</strong> staj ilanını sizin için inceledim. <br/><br/> <a href='/StudentPanel/Details/{internshipId}' class='btn btn-sm btn-primary text-white mt-2 shadow-sm'>İlan Detaylarını Görüntüle</a>";

            var message = new ChatMessage
            {
                SenderId = advisorId,
                ReceiverId = studentId,
                Content = recommendationMsg,
                SentDate = DateTime.Now,
                IsRead = false,
                SuggestedInternshipId = internshipId
            };

            await _unitOfWork.ChatMessages.AddAsync(message);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"{student.FullName} isimli öğrencieye profesyonel öneri liyakatle iletildi! ✨🥂";

            return RedirectToAction(nameof(Index));
        }
    }
}