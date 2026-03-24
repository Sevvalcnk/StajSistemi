using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;

namespace StajSistemi.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Bölümleri Listeleme (Hafta 4)
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
        }

        // Yeni Bölüm Ekleme Sayfası
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Bölüm Kaydetme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }
    }
}