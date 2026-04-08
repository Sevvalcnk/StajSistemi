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

        // --- 1. DASHBOARD (ANA PANEL) ---
        public async Task<IActionResult> Index()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var allInternships = await _unitOfWork.Internships.GetAllIncludingAsync(i => i.Department, i => i.City);

            var smartMatches = allInternships
                .Where(i => !i.IsDeleted && i.Status == "Aktif")
                .Select(ilan => new
                {
                    Ilan = ilan,
                    Score = (ilan.DepartmentId == studentDto.DepartmentId ? 50 : 0) +
                            (ilan.CityId == studentDto.CityId ? 30 : 0) +
                            (studentDto.GPA >= 3.0 ? 15 : (studentDto.GPA >= 2.0 ? 5 : 0)) +
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

        // --- BAŞVURULARIM SAYFASI ---
        public async Task<IActionResult> Applications()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.Internship);
            var myApplications = apps.Where(a => a.AppUserId == studentDto.Id && !a.IsDeleted).ToList();

            return View(myApplications);
        }

        // --- STAJ SONUÇLARIM SAYFASI ---
        public async Task<IActionResult> Results()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");

            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(a => a.Internship);
            var myResults = apps.Where(a => a.AppUserId == studentDto.Id && (a.Status == "Onaylandı" || a.Status == "Reddedildi")).ToList();

            return View(myResults);
        }

        // --- PROFİL DÜZENLEME (GET) ---
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

        // --- PROFİL DÜZENLEME (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(StudentDto studentDto, IFormFile? CVFile, IFormFile? CertificateFile)
        {
            ModelState.Remove("Department");
            ModelState.Remove("DepartmentName");
            ModelState.Remove("City");

            if (ModelState.IsValid)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentDto.Id);
                if (student == null) return NotFound();

                student.FirstName = studentDto.Name;
                student.LastName = studentDto.Surname;
                student.GPA = studentDto.GPA;
                student.PhoneNumber = studentDto.PhoneNumber;
                student.PersonalSkills = studentDto.PersonalSkills;
                student.EducationSummary = studentDto.EducationSummary;
                student.DepartmentId = studentDto.DepartmentId;
                student.CityId = studentDto.CityId;

                if (CVFile != null && CVFile.Length > 0)
                    student.CVPath = "/uploads/cvs/" + await SaveFile(CVFile, "cvs");

                if (CertificateFile != null && CertificateFile.Length > 0)
                    student.CertificatePath = "/uploads/certs/" + await SaveFile(CertificateFile, "certs");

                _unitOfWork.Students.Update(student);
                await _unitOfWork.SaveAsync();

                TempData["SuccessMessage"] = "Profiliniz mühürlendi! ✨";
                return RedirectToAction(nameof(Index));
            }
            return View(studentDto);
        }

        // --- 🛠️ YARDIMCI METOTLAR ---
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

        private async Task<StudentDto> GetLoggedInStudentDto()
        {
            var currentUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserName)) return null;

            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department, s => s.City);
            var student = students.FirstOrDefault(s => s.UserName == currentUserName || s.Email == currentUserName);

            if (student == null) return null;
            return _mapper.Map<StudentDto>(student);
        }

        public async Task<IActionResult> About() => View(await GetLoggedInStudentDto());
        public async Task<IActionResult> Contact() => View(await GetLoggedInStudentDto());
        public async Task<IActionResult> Documents() => View(await GetLoggedInStudentDto());

        // ✅ İSMİ DÜZELTİLDİ: Artık Linklerdeki "Details" ile tam uyumlu!
        public async Task<IActionResult> Details(int id)
        {
            // 1. Veritabanından ilişkili tablolarla beraber çekiyoruz
            var apps = await _unitOfWork.InternshipApplications.GetAllIncludingAsync(
                a => a.Internship,
                a => a.Internship.Department,
                a => a.Internship.City);

            // 2. Tıklanan ID'ye göre başvuruyu buluyoruz
            var application = apps.FirstOrDefault(a => a.Id == id);

            if (application == null) return NotFound();

            return View(application);
        }
    }
}