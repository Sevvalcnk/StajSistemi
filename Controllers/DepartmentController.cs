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
    [Authorize(Roles = "Advisor,Admin")] // Sadece yetkililer girebilir
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

        // --- 🗑️ BÖLÜM SİLME (GÜVENLİ HALE GETİRİLDİ) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
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
            }
            catch (Exception)
            {
                // 🛡️ SİBER KORUMA: Eğer bölüme kayıtlı öğrenciler varsa veritabanı hata verir.
                // Bu catch bloğu o hatayı yakalar ve sistemi çökertmek yerine uyarı verir.
                TempData["ErrorMessage"] = "Bu bölüme kayıtlı aktif öğrenciler olduğu için silme işlemi gerçekleştirilemedi! 🚫";
            }

            return RedirectToAction(nameof(Index));
        }

        // --- 🛡️ AKILLI SÜZGEÇ (TAM KAPSAYICI - PROFESYONEL VERSİYON) ---
        [HttpGet]
        [AllowAnonymous]
        [Route("/Department/GetDepartmentsByLevel")]
        public async Task<JsonResult> GetDepartmentsByLevel(string level)
        {
            try
            {
                // 1. Önce bütün bölümleri bir sorgu olarak hazırla
                var allDepts = await _context.Departments.ToListAsync();

                // 2. 🚀 SİBER MANTIK: Gelen seviyeye göre isim tabanlı filtreleme (Hiçbir bölüm dışarıda kalmaz)
                var searchLevel = (level ?? "").Trim();
                IEnumerable<Department> filtered;

                if (searchLevel == "Önlisans")
                {
                    // 2 Yıllık anahtarları: Programı, Teknolojileri, Meslek, Sekreterlik, Muhasebe
                    filtered = allDepts.Where(d =>
                        d.DepartmentName.Contains("Programı") ||
                        d.DepartmentName.Contains("Teknolojileri") ||
                        d.DepartmentName.Contains("Meslek") ||
                        d.DepartmentName.Contains("Sekreterlik") ||
                        d.DepartmentName.Contains("Muhasebe"));
                }
                else // Lisans veya diğer durumlar
                {
                    // Yukarıdaki ibareleri içermeyen HER ŞEYİ Lisans kabul et (Tam Kapsayıcı)
                    filtered = allDepts.Where(d =>
                        !d.DepartmentName.Contains("Programı") &&
                        !d.DepartmentName.Contains("Teknolojileri") &&
                        !d.DepartmentName.Contains("Sekreterlik") &&
                        !d.DepartmentName.Contains("Muhasebe"));
                }

                // 3. Verileri topla ve JSON paketini mühürle
                var result = filtered
                    .OrderBy(d => d.DepartmentName)
                    .Select(d => new { id = d.Id, departmentName = d.DepartmentName })
                    .ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, message = ex.Message });
            }
        }
    }
}