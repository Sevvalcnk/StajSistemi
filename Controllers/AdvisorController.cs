using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // --- ✅ 1. ÖĞRENCİ LİSTESİ (Dokunulmadı, Filtreleme Doğru) ---
        public async Task<IActionResult> Index(int? cityId, int? departmentId, double? minGpa)
        {
            var allStudents = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var allApps = await _unitOfWork.InternshipApplications.GetAllAsync();

            var studentQuery = allStudents.Where(s => s.IsDeleted == false);

            if (cityId.HasValue) studentQuery = studentQuery.Where(s => s.CityId == cityId);
            if (departmentId.HasValue) studentQuery = studentQuery.Where(s => s.DepartmentId == departmentId);
            if (minGpa.HasValue) studentQuery = studentQuery.Where(s => s.GPA >= minGpa);

            var filteredStudents = studentQuery.OrderByDescending(s => s.GPA).ToList();

            ViewBag.NewApplicationsCount = allApps.Count(a => a.Status == "Beklemede" && !a.IsDeleted);

            var cities = await _unitOfWork.Cities.GetAllAsync();
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Cities = new SelectList(cities.OrderBy(c => c.Name), "Id", "Name", cityId);
            ViewBag.Departments = new SelectList(departments.OrderBy(d => d.DepartmentName), "Id", "DepartmentName", departmentId);

            var studentDtos = _mapper.Map<List<StudentDto>>(filteredStudents);

            foreach (var student in studentDtos)
            {
                var lastApp = allApps
                    .Where(a => a.AppUserId == student.Id && !a.IsDeleted)
                    .OrderByDescending(a => a.ApplicationDate)
                    .FirstOrDefault();

                student.InternshipStatus = lastApp != null ? lastApp.Status : "Başvuru Yok";
            }

            return View(studentDtos);
        }

        // --- ✅ 2. ÖĞRENCİ DETAYI (Düzenlendi: İlişkili Tablolar Eklendi) ---
        public async Task<IActionResult> StudentDetails(int id)
        {
            // Mühür: Detay sayfasında bölüm ve şehir adının görünmesi için Include ekledik
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.Id == id);

            if (student == null || student.IsDeleted)
            {
                TempData["ErrorMessage"] = "Öğrenci bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            var studentDto = _mapper.Map<StudentDto>(student);
            return View(studentDto);
        }

        // --- ✅ 3. SOFT DELETE (Dokunulmadı) ---
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

        // --- ✅ 4. STAJ BAŞVURULARI LİSTESİ (Dokunulmadı) ---
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

        // --- 🔥 5. YENİ MÜHÜR: BAŞVURU DURUMUNU GÜNCELLE (Onay/Red) ---
        // Bu metot, hocanın butona bastığında başvuruyu onaylamasını sağlar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, string status)
        {
            var app = await _unitOfWork.InternshipApplications.GetByIdAsync(id);
            if (app == null) return NotFound();

            app.Status = status; // "Onaylandı" veya "Reddedildi"

            _unitOfWork.InternshipApplications.Update(app);
            await _unitOfWork.SaveAsync();

            TempData["SuccessMessage"] = $"Başvuru '{status}' olarak mühürlendi! ✨";

            // İşlem bitince listeye geri dön
            return RedirectToAction(nameof(InternshipApplications));
        }
    }
}