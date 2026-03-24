using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract; // ✅ HomeController ile aynı yaptık

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Advisor,Admin")] // Admin de görebilsin diye esnettik
    public class AdvisorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdvisorController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // --- 1. ÖĞRENCİ LİSTESİ ---
        public async Task<IActionResult> Index()
        {
            var allStudents = await _unitOfWork.Students.GetAllAsync();
            var activeStudents = allStudents.Where(s => s.IsDeleted == false).ToList();

            // ✅ BİLDİRİM: Sol menüdeki rakamın güncel kalması için
            var apps = await _unitOfWork.InternshipApplications.GetAllAsync();
            ViewBag.NewApplicationsCount = apps.Count(a => a.Status == "Beklemede" && !a.IsDeleted);

            var studentDtos = _mapper.Map<List<StudentDto>>(activeStudents);
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
                TempData["SuccessMessage"] = "Kayıt başarıyla arşive kaldırıldı. 🥂";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 4. STAJ BAŞVURULARI LİSTELEME (ONAY/RED EKRANI) ---
        public async Task<IActionResult> InternshipApplications()
        {
            var applications = await _unitOfWork.InternshipApplications
                .GetAllIncludingAsync(a => a.AppUser, a => a.Internship);

            var activeApplications = applications
                .Where(a => a.IsDeleted == false)
                .OrderByDescending(a => a.ApplicationDate)
                .ToList();

            // ✅ BİLDİRİM: Sayfayı yenileyince baloncuğun güncellenmesi için
            ViewBag.NewApplicationsCount = activeApplications.Count(a => a.Status == "Beklemede");

            return View(activeApplications);
        }
    }
}