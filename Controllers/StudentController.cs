using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract; // ✅ DÜZELTME: Eski 'UnitOfWork' using'i silindi, 'Abstract' eklendi.

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Advisor,Admin")]
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // 1. Listeleme: ✅ MÜHÜR: Soft Delete filtresi burada çalışıyor!
        public async Task<IActionResult> Index()
        {
            // Identity kullandığımız için Students deposu artık AppUser döndürüyor.
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department);

            // ✅ KRİTİK FİLTRE: Sadece silinmemiş (IsDeleted == false) olanları alıyoruz.
            var activeStudents = students.Where(s => !s.IsDeleted).ToList();

            var studentDtos = _mapper.Map<List<StudentDto>>(activeStudents);
            return View(studentDtos);
        }

        // 2. Ekleme Sayfası (GET)
        public async Task<IActionResult> Create()
        {
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");
            return View();
        }

        // 3. Kayıt İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken] // ✅ GÜVENLİK MÜHÜRÜ: CSRF saldırılarına karşı eklendi
        public async Task<IActionResult> Create(StudentDto studentDto)
        {
            if (ModelState.IsValid)
            {
                // Identity uyumu için StudentDto'yu AppUser (veya Student) modeline mapliyoruz
                var student = _mapper.Map<AppUser>(studentDto);

                student.IsDeleted = false;

                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.SaveAsync();

                TempData["SuccessMessage"] = "Yeni öğrenci başarıyla mühürlendi! 🥂";
                return RedirectToAction(nameof(Index));
            }

            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");
            return View(studentDto);
        }
    }
}