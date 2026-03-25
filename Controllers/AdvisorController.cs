using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;

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

        // --- ✅ 1. ÖĞRENCİ LİSTESİ (Geliştirilmiş ve Bağlanmış Versiyon) ---
        public async Task<IActionResult> Index()
        {
            // 1. Tüm öğrencileri ve tüm başvuruları aynı anda çekiyoruz
            var allStudents = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department);
            var allApps = await _unitOfWork.InternshipApplications.GetAllAsync();

            var activeStudents = allStudents.Where(s => s.IsDeleted == false).ToList();

            // 2. Bildirim sayısını ViewBag'e mühürle
            ViewBag.NewApplicationsCount = allApps.Count(a => a.Status == "Beklemede" && !a.IsDeleted);

            // 3. Öğrencileri DTO'ya çevir
            var studentDtos = _mapper.Map<List<StudentDto>>(activeStudents);

            // 🔥 KRİTİK ADIM: Her öğrencinin staj durumunu başvurular tablosundan bulup DTO'ya mühürlüyoruz
            foreach (var student in studentDtos)
            {
                // Bu öğrenciye ait en son (ve silinmemiş) başvuruyu bul
                var lastApp = allApps
                    .Where(a => a.AppUserId == student.Id && !a.IsDeleted)
                    .OrderByDescending(a => a.ApplicationDate)
                    .FirstOrDefault();

                // Eğer başvurusu varsa durumunu yaz, yoksa "Başvuru Yok" de
                student.InternshipStatus = lastApp != null ? lastApp.Status : "Başvuru Yok";
            }

            return View(studentDtos);
        }

        // --- 2. ÖĞRENCİ DETAYI ---
        public async Task<IActionResult> StudentDetails(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null || student.IsDeleted)
            {
                TempData["ErrorMessage"] = "Öğrenci bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            var studentDto = _mapper.Map<StudentDto>(student);
            return View(studentDto);
        }

        // --- 3. SOFT DELETE ---
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
                TempData["SuccessMessage"] = "Öğrenci kaydı başarıyla arşive kaldırıldı. 🥂";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 4. STAJ BAŞVURULARI (Onay/Red Ekranı) ---
        public async Task<IActionResult> InternshipApplications()
        {
            var applications = await _unitOfWork.InternshipApplications
                .GetAllIncludingAsync(a => a.AppUser, a => a.Internship);

            var activeApplications = applications
                .Where(a => a.IsDeleted == false)
                .OrderByDescending(a => a.ApplicationDate)
                .ToList();

            ViewBag.NewApplicationsCount = activeApplications.Count(a => a.Status == "Beklemede");

            return View(activeApplications);
        }
    }
}