using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.UnitOfWork;

namespace StajSistemi.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // 1. Listeleme: Bölüm bilgisini de dahil ederek çekiyoruz
        public async Task<IActionResult> Index()
        {
            // --- KRİTİK DEĞİŞİKLİK BURASI ---
            // GetAllIncludingAsync kullanarak öğrencinin bağlı olduğu "Department" verisini de yüklüyoruz.
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department);
            
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            return View(studentDtos);
        }

        // 2. Ekleme Sayfası (GET)
        [Authorize(Roles = "Advisor,Admin")]
        public async Task<IActionResult> Create()
        {
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");
            return View();
        }

        // 3. Kayıt İşlemi (POST)
        [HttpPost]
        [Authorize(Roles = "Advisor,Admin")]
        public async Task<IActionResult> Create(StudentDto studentDto)
        {
            if (ModelState.IsValid)
            {
                var student = _mapper.Map<Student>(studentDto);
                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");
            return View(studentDto);
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department);
        }
    }
}