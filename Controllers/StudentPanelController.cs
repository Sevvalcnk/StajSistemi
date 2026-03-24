using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore; // ✅ Include ve ToListAsync için şart

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

        // --- 1. DASHBOARD ---
        public async Task<IActionResult> Index()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return RedirectToAction("Login", "Account");
            return View(studentDto);
        }

        // --- 2. PROFİL DÜZENLEME (GET) ---
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var studentDto = await GetLoggedInStudentDto();
            if (studentDto == null) return NotFound();

            // ✅ MÜHÜR: Sayfa açılırken bölümleri listeye dolduruyoruz
            ViewBag.Departments = await _unitOfWork.Departments.GetAllAsync();

            return View(studentDto);
        }

        // --- 3. PROFİL DÜZENLEME (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(StudentDto studentDto, IFormFile? CVFile, IFormFile? CertificateFile)
        {
            // Validasyon temizliği
            ModelState.Remove("Department");
            ModelState.Remove("DepartmentName");

            if (ModelState.IsValid)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentDto.Id);
                if (student == null) return NotFound();

                // ✅ KRİTİK EŞLEŞME: Formdan gelen verileri modele mühürlüyoruz
                student.FirstName = studentDto.Name;
                student.LastName = studentDto.Surname;
                student.GPA = studentDto.GPA;
                student.PhoneNumber = studentDto.PhoneNumber;
                student.PersonalSkills = studentDto.PersonalSkills;
                student.EducationSummary = studentDto.EducationSummary;

                // ✅ YENİ: Seçilen bölümü kaydediyoruz
                student.DepartmentId = studentDto.DepartmentId;

                // 📁 CV Yükleme İşlemi
                if (CVFile != null && CVFile.Length > 0)
                {
                    string uniqueFileName = await SaveFile(CVFile, "cvs");
                    student.CVPath = "/uploads/cvs/" + uniqueFileName;
                }

                // 📁 Sertifika Yükleme İşlemi
                if (CertificateFile != null && CertificateFile.Length > 0)
                {
                    string uniqueCertName = await SaveFile(CertificateFile, "certs");
                    student.CertificatePath = "/uploads/certs/" + uniqueCertName;
                }

                try
                {
                    _unitOfWork.Students.Update(student);
                    await _unitOfWork.SaveAsync();

                    TempData["SuccessMessage"] = "Profiliniz ve bölümünüz başarıyla mühürlendi! ✨";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Hata: " + ex.Message);
                }
            }

            // Hata varsa bölümleri tekrar yükle
            ViewBag.Departments = await _unitOfWork.Departments.GetAllAsync();
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

            // Giriş yapan öğrenciyi Bölüm bilgisiyle beraber getiriyoruz
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department);
            var student = students.FirstOrDefault(s => s.UserName == currentUserName || s.Email == currentUserName);

            if (student == null) return null;
            return _mapper.Map<StudentDto>(student);
        }

        // Diğer sayfalar
        public async Task<IActionResult> About() => View(await GetLoggedInStudentDto());
        public async Task<IActionResult> Contact() => View(await GetLoggedInStudentDto());
        public async Task<IActionResult> Documents() => View(await GetLoggedInStudentDto());
    }
}