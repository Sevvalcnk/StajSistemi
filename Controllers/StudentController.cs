using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için gerekli
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.UnitOfWork;

namespace StajSistemi.Controllers
{
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // 1. Listeleme
        public async Task<IActionResult> Index()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            return View(studentDtos);
        }

        // 2. Ekleme Sayfasını Açan Kısım (GET)
        // 'async Task' ekledik çünkü bölümleri veritabanından çekeceğiz
        public async Task<IActionResult> Create()
        {
            // Veritabanındaki tüm bölümleri çekiyoruz
            var departments = await _unitOfWork.Departments.GetAllAsync();

            // Çektiğimiz bölümleri ViewBag ile sayfadaki dropdown'a (açılır liste) gönderiyoruz
            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");

            return View();
        }

        // 3. Formdan Gelen Veriyi Kaydeden Kısım (POST)
        [HttpPost]
        public async Task<IActionResult> Create(StudentDto studentDto)
        {
            if (ModelState.IsValid)
            {
                var student = _mapper.Map<Student>(studentDto);
                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            // Eğer formda hata varsa (geçersiz model durumu), 
            // sayfayı tekrar yüklemeden önce bölümleri yeniden doldurmalıyız 
            // yoksa yine 'items' hatası alırsın!
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");

            return View(studentDto);
        }
    }
}