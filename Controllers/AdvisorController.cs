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

            // 🛡️ SİBER RADAR: 30 gün kontrolü için tüm raporları çekiyoruz
            var allDailyReports = await _unitOfWork.DailyReports.GetAllAsync();

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

            // ✅ SİBER GÜNCELLEME: Mükerrer bölümleri (GroupBy) engelliyoruz
            ViewBag.Departments = new SelectList(
                departments.GroupBy(d => d.DepartmentName).Select(g => g.First()).OrderBy(d => d.DepartmentName),
                "Id", "DepartmentName", departmentId);

            var studentDtos = _mapper.Map<List<StudentDto>>(filteredStudents);

            // 🚀 SİBER SİNYAL SİSTEMİ: Her öğrenci için rapor sayısını hesapla
            var needsApprovalList = new Dictionary<int, bool>();
            var reportCounts = new Dictionary<int, int>();

            foreach (var student in studentDtos)
            {
                var originalStudent = filteredStudents.First(s => s.Id == student.Id);
                student.StudentNo = !string.IsNullOrWhiteSpace(originalStudent.StudentNo) ? originalStudent.StudentNo : originalStudent.UserName;

                var lastApp = allApps
     .Where(a => a.AppUserId == student.Id && !a.IsDeleted)
     .OrderByDescending(a => a.Id) // 🚀 KRİTİK: Tarih yerine ID ile en yeniyi garantile!
     .FirstOrDefault();

                if (lastApp != null)
                {
                    student.InternshipStatus = lastApp.Status switch
                    {
                        // ✨ AYRIM: Bitiş tarihi yoksa 'Staj Başladı' de, varsa 'Onaylandı' de.
                        ApplicationStatus.Approved => lastApp.CompletedDate == null ? "Staj Başladı" : "Onaylandı",
                        ApplicationStatus.Rejected => "Reddedildi",
                        ApplicationStatus.Pending => "Beklemede",
                        _ => "İşlemde"
                    };

                    // 🚀 MÜHÜR: Sadece BU başvurunun raporlarını say, eskileri karıştırma!
                    int reportCount = allDailyReports.Where(r => r.InternshipApplicationId == lastApp.Id).Select(r => r.DayNumber).Distinct().Count();

                    // Lamba sadece 30 gün dolduğunda VE staj henüz bitirilmediyse yanar
                    bool needsFinalApproval = reportCount >= 30 && lastApp.CompletedDate == null && student.InternshipStatus != "Onaylandı";
                    

                    needsApprovalList.Add(student.Id, needsFinalApproval);
                    reportCounts.Add(student.Id, reportCount);
                }
                else { student.InternshipStatus = "Başvuru Yok"; }
            }

            ViewBag.NeedsApprovalList = needsApprovalList;
            ViewBag.ReportCounts = reportCounts;

            return View(studentDtos);
        }

        // 🛡️ 🚀 GÜNCELLENDİ: AKILLI BÖLÜM ÇEKME MOTORU (UYUŞMAZLIK VE MÜKERRER KAYIT ÇÖZÜLDÜ)
        [HttpGet]
        public async Task<JsonResult> GetDepartmentsByLevel(string level)
        {
            var allDepartments = await _unitOfWork.Departments.GetAllAsync();

            if (string.IsNullOrEmpty(level) || level == "Hepsi")
            {
                // Mükerrerleri engellemek için GroupBy ekledim kral
                var all = allDepartments.GroupBy(d => d.DepartmentName).Select(g => g.First()).OrderBy(d => d.DepartmentName).Select(d => new { id = d.Id, name = d.DepartmentName }).ToList();
                return Json(all);
            }

            // 🛡️ SİBER NORMALİZASYON: "Önlisans" ve "Ön Lisans" arasındaki boşluk farkını yok ediyoruz
            string clean = level.Replace(" ", "").ToLower();

            var filtered = allDepartments
                .Where(d => d.DepartmentName != null && (
                    d.DepartmentName.Replace(" ", "").ToLower().Contains(clean) ||
                    // 🎯 AKILLI TAHMİN: İsminde "Mühendisliği/Yönetimi" varsa Lisanstır, "Programı/Teknolojileri" varsa Önlisanstır.
                    (clean.Contains("lisans") && !clean.Contains("ön") && (d.DepartmentName.Contains("Mühendisliği") || d.DepartmentName.Contains("Mimarlık") || d.DepartmentName.Contains("Yönetimi") || d.DepartmentName.Contains("Öğretmenliği"))) ||
                    (clean.Contains("ön") && (d.DepartmentName.Contains("Programı") || d.DepartmentName.Contains("Teknolojileri") || d.DepartmentName.Contains("Hizmetleri") || d.DepartmentName.Contains("Sekreterlik")))
                ))
                .GroupBy(d => d.DepartmentName).Select(g => g.First()) // 🛡️ MÜHÜR: Tekil bölümler
                .OrderBy(d => d.DepartmentName)
                .Select(d => new { id = d.Id, name = d.DepartmentName })
                .ToList();

            return Json(filtered);
        }

        // 🛡️ 🚀 GÜNCELLENDİ: ARŞİV FİLTRELEME (NULL, MÜKERRER VE GRADE UYUŞMAZLIK FİX)
        public async Task<IActionResult> Archive(int? departmentId, string grade)
        {
            var allStudents = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var archivedQuery = allStudents.Where(s => s.IsDeleted == true);

            // 🏛️ 1. ADIM: Eğer bir bölüm seçilmişse, direkt ona odaklan
            if (departmentId.HasValue)
            {
                archivedQuery = archivedQuery.Where(s => s.DepartmentId == departmentId);
            }
            // 🏛️ 2. ADIM: Grade (Eğitim Düzeyi) varsa akıllı eşleşme yap
            else if (!string.IsNullOrEmpty(grade) && grade != "Hepsi")
            {
                string clean = grade.Replace(" ", "").ToLower();

                // ✅ SİBER ONARIM: s.Grade != null kontrolü (s.Grade ?? "") ile yapıldı, NullReference mühürlendi
                archivedQuery = archivedQuery.Where(s =>
                    ((s.Grade ?? "").Replace(" ", "").ToLower().Contains(clean)) ||
                    (s.Department != null && (
                        s.Department.DepartmentName.Replace(" ", "").ToLower().Contains(clean) ||
                        (clean.Contains("lisans") && !clean.Contains("ön") && (s.Department.DepartmentName.Contains("Mühendisliği") || s.Department.DepartmentName.Contains("Yönetimi"))) ||
                        (clean.Contains("ön") && (s.Department.DepartmentName.Contains("Programı") || s.Department.DepartmentName.Contains("Teknolojileri")))
                    ))
                );
            }

            var archivedStudents = archivedQuery.OrderByDescending(s => s.FullName).ToList();
            var studentDtos = _mapper.Map<List<StudentDto>>(archivedStudents);

            var departments = await _unitOfWork.Departments.GetAllAsync();
            // ✅ SİBER GÜNCELLEME: Arşiv dropdown mükerrer engelleme
            ViewBag.Departments = new SelectList(
                departments.GroupBy(d => d.DepartmentName).Select(g => g.First()).OrderBy(d => d.DepartmentName),
                "Id", "DepartmentName", departmentId);

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

        // --- 🛡️ 🚀 GÜNCELLENDİ: STAJ DOSYASI GÖRÜNTÜLEME (KAPAK BİLGİ KAYBI FİXLENDİ) ---
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

            // 🚀 SİBER DÜZELTME: Sadece 'Approved' olanı değil, 'Rejected' olsa bile son başvuruyu getiriyoruz ki kapak dolu gelsin!
            var activeApp = apps.Where(a => a.AppUserId == studentId && !a.IsDeleted)
                                .OrderByDescending(a => a.ApplicationDate)
                                .FirstOrDefault();

            ViewBag.ActiveInternship = activeApp;

            var reports = await _unitOfWork.DailyReports.GetAllAsync();
            var studentReports = reports.Where(r => r.AppUserId == studentId).OrderBy(r => r.DayNumber).ToList();

            // 🛡️ KRİTİK VERİ: 31/30 Distinct Mührü
            int writtenCount = studentReports.Select(r => r.DayNumber).Distinct().Count();
            ViewBag.ApprovedReportsCount = writtenCount;
            ViewBag.CanApproveInternship = writtenCount >= 30;

            ViewBag.DailyReports = studentReports;

            // 🚀 TASARIM MÜHRÜ: StudentPanel tasarımını giydiriyoruz.
            return View("../StudentPanel/Documents", studentDto);
        }

        public async Task<IActionResult> ViewStudentDashboard(int studentId)
        {
            // 🛡️ 1. ADIM: Önce öğrenciyi veritabanından çekiyoruz (GPA hatasını çözen satır!)
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) return NotFound();

            // 🚀 2. DTO Hazırlığı ve Bölüm Bilgisi
            var studentDto = _mapper.Map<StudentDto>(student);
            studentDto.StudentNo = !string.IsNullOrWhiteSpace(student.StudentNo) ? student.StudentNo : student.UserName;

            var departments = await _unitOfWork.Departments.GetAllAsync();
            studentDto.DepartmentName = departments.FirstOrDefault(d => d.Id == student.DepartmentId)?.DepartmentName ?? "Bölüm Belirtilmemiş";

            // 📊 3. Rapor İstatistikleri
            var allDailyReports = await _unitOfWork.DailyReports.GetAllAsync();
            var myReports = allDailyReports.Where(r => r.AppUserId == studentId).ToList();

            int totalReportsCount = myReports.Select(r => r.DayNumber).Distinct().Count();
            ViewBag.TotalReportsCount = totalReportsCount;
            ViewBag.ApprovedReportsCount = totalReportsCount;
            ViewBag.PendingReportsCount = 0;
            ViewBag.ProgressPercent = Math.Round(Math.Min(((double)totalReportsCount / 30) * 100, 100), 0);
            ViewBag.RemainingDays = Math.Max(30 - totalReportsCount, 0);

            // 🛡️ 4. Zaman Çizelgesi (Timeline) İçin En Yeni Başvuruyu Çekme
            var allApps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.Internship);

            var activeAppForTimeline = allApps
                .Where(a => a.AppUserId == studentId && !a.IsDeleted &&
                           (a.Status == ApplicationStatus.Approved ||
                            a.Status == ApplicationStatus.Rejected ||
                            a.Status == ApplicationStatus.Pending))
                .OrderByDescending(a => a.Id)
                .FirstOrDefault();

            ViewBag.ActiveAppForTimeline = activeAppForTimeline;
            ViewBag.AppliedInternshipIds = allApps.Where(a => a.AppUserId == studentId).Select(a => a.InternshipId).ToList();

            // ✅ SİBER MÜHÜR: GPA boş olsa bile artık çökmez!
            ViewBag.TotalScore = (int)((student.GPA ?? 0) * 100);

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
            TempData["SuccessMessage"] = $"Başvuru başarıyla '{mesaj}' olarak tescil edildi! ✨🥂";

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
                TempData["SuccessMessage"] = $"{dayNumber}. Gün Raporu Onaylandı! 🛡️🥂";
            }
            else
            {
                TempData["ErrorMessage"] = "Onaylanacak rapor içeriği bulunamadı. ❌";
            }

            return RedirectToAction(nameof(ViewStudentFile), new { studentId = studentId });
        }

        // --- 🛡️ 🚀 GÜNCELLENDİ: STAJ DEFTERİNİ KOMPLE ONAYLA (REDDEDİLENİ TEKRAR ONAYLAMA FİXİ) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFullInternship(int studentId)
        {
            var reports = await _unitOfWork.DailyReports.GetAllAsync();
            // 🛡️ 31/30 ÇÖZÜMÜ BURADA DA ŞART
            int writtenCount = reports.Where(r => r.AppUserId == studentId).Select(r => r.DayNumber).Distinct().Count();

            if (writtenCount < 30)
            {
                return Json(new { success = false, message = $"Süreç Uyarısı: 30 iş günü tamamlanmadan resmi onay verilemez. (Mevcut: {writtenCount}/30) 🚫" });
            }

            var apps = await _unitOfWork.InternshipApplications.GetAllAsync();

            // 🚀 KRİTİK SİBER DÜZELTME: Reddedilmiş (Rejected) veya Beklemede (Pending) olsa bile kaydı bulup 'Approved' yapabilmek için sorguyu mühürledik!
            var activeApp = apps.FirstOrDefault(a => a.AppUserId == studentId
                                                  && !a.IsDeleted
                                                  && (a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Rejected || a.Status == ApplicationStatus.Pending));

            if (activeApp != null)
            {
                activeApp.CompletedDate = DateTime.Now;
                activeApp.Status = ApplicationStatus.Approved; // Reddedilmişse bile asaletle Onaylandıya çekiyoruz!

                var studentReports = reports.Where(r => r.AppUserId == studentId).ToList();
                foreach (var r in studentReports) { r.IsApproved = true; } // Toplu onay

                await _unitOfWork.SaveAsync();

                return Json(new { success = true, message = "Staj süreci başarıyla tamamlandı ve sistem kayıtlarına resmi olarak işlendi! 🛡️⚓🥂" });
            }

            return Json(new { success = false, message = "Onaylanacak aktif veya reddedilmiş bir staj kaydı bulunamadı! ❌" });
        }

        // --- 🛡️ 🚀 GÜNCELLENDİ: STAJ DEFTERİNİ KOMPLE REDDET ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFullInternship(int studentId, string rejectionReason)
        {
            var apps = await _unitOfWork.InternshipApplications.GetAllAsync();
            var activeApp = apps.Where(a => a.AppUserId == studentId && !a.IsDeleted)
                                .OrderByDescending(a => a.Id).FirstOrDefault();

            if (activeApp != null)
            {
                // ✅ 1. MÜHÜR: Gerekçeyi başvuruya kaydediyoruz
                activeApp.Status = ApplicationStatus.Rejected;
                activeApp.RejectionReason = rejectionReason;
                activeApp.CompletedDate = null;
                _unitOfWork.InternshipApplications.Update(activeApp);

                // ✅ 2. SİNYAL: Mesaj kutusuna (Chat) bildirim gönderiyoruz
                var advisorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int advisorId = int.TryParse(advisorIdStr, out var id) ? id : 0;

                var chatMsg = new ChatMessage
                {
                    SenderId = advisorId,
                    ReceiverId = studentId,
                    Content = $"🚫 <strong>STAJINIZ REDDEDİLDİ.</strong><br/><strong>Hoca Notu:</strong> {rejectionReason}<br/>Süreci sıfırlayıp yeni bir başlangıç yapabilirsiniz.",
                    SentDate = DateTime.Now
                };
                await _unitOfWork.ChatMessages.AddAsync(chatMsg);

                await _unitOfWork.SaveAsync();
                TempData["ErrorMessage"] = "Staj reddedildi ve öğrenciye bilgi verildi. 🚫";
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

            TempData["SuccessMessage"] = $"{student.FullName} isimli öğrenciye profesyonel öneri resmi olarak iletildi! ✨🥂";

            return RedirectToAction(nameof(Index));
        }
    }
}