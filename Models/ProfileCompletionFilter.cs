using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Linq;
using StajSistemi.data;

namespace StajSistemi.Filters
{
    public class ProfileCompletionFilter : IActionFilter
    {
        private readonly ApplicationDbContext _context;

        public ProfileCompletionFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 👮‍♂️ 1. ADIM: Neredeyiz?
            var actionName = context.RouteData.Values["action"]?.ToString();
            var controllerName = context.RouteData.Values["controller"]?.ToString();

            // 🛡️ 2. ADIM: SONSUZ DÖNGÜ VE AJAX KIRICI (SİBER BAYPAS)
            // Eğer öğrenci zaten "Profil Düzenleme" sayfasındaysa, çıkış yapıyorsa 
            // VEYA dropdown için "Department" verisi çekiyorsa filtreyi DURDUR!
            if ((controllerName == "StudentPanel" && actionName == "EditProfile") ||
                 controllerName == "Department" ||  // 🚀 İŞTE KRİTİK EKLEME BURASI!
                 controllerName == "Account" ||
                 actionName == "Error")
            {
                return; // Polis: "Geçiş serbest, bu güvenli bir yol" diyor.
            }

            // 👮‍♂️ 3. ADIM: Kimlik ve Rol Kontrolü
            if (context.HttpContext.User.Identity != null &&
                context.HttpContext.User.Identity.IsAuthenticated &&
                context.HttpContext.User.IsInRole("Student"))
            {
                var userIdString = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(userIdString, out int userId))
                {
                    // 🔍 4. ADIM: Veritabanından bilgileri kontrol et
                    var student = _context.Users.FirstOrDefault(s => s.Id == userId);

                    // Bölüm veya Üniversite bilgisi boş mu?
                    if (student != null && (student.DepartmentId == null || student.DepartmentId == 0 || string.IsNullOrEmpty(student.UniversityName)))
                    {
                        // 🔐 5. ADIM: KİLİTLEME VE YÖNLENDİRME
                        // AJAX isteklerini kontrol et: Eğer bir AJAX isteği ise yönlendirme yapma, 401 dön.
                        if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return;
                        }

                        context.Result = new RedirectToActionResult("EditProfile", "StudentPanel", null);
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}