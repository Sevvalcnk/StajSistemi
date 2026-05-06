using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data;
using StajSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Advisor,Admin")] // Sadece yetkililer bu odaya girebilir
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 📋 BÖLÜMLERİ LİSTELEME ---
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
        }

        // --- ➕ YENİ BÖLÜM EKLEME (GET) ---
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // --- ➕ YENİ BÖLÜM EKLEME (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{department.DepartmentName} bölümü siber kayıtlara mühürlendi! 🥂✨";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // --- 🗑️ BÖLÜM SİLME (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bölüm siber tarihten başarıyla silindi! 🛡️🗑️";
            }
            else
            {
                TempData["ErrorMessage"] = "Silinecek bölüm bulunamadı! 🚫";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 🛡️ AJAX METODU (Mevcut kodun, korundu) ---
        [HttpGet]
        [AllowAnonymous]
        [Route("/Department/GetDepartmentsByLevel")]
        public async Task<JsonResult> GetDepartmentsByLevel()
        {
            try
            {
                var allDepartments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .Select(d => new { id = d.Id, departmentName = d.DepartmentName })
                    .ToListAsync();
                return Json(allDepartments);
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, message = ex.Message });
            }
        }
    }
}