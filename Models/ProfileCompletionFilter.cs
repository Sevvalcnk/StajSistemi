using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Linq;
using StajSistemi.data; // Küçük 'd' mühürü korundu

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

            // 🛡️ 2. ADIM: SONSUZ DÖNGÜ KIRICI (KRİTİK)
            // Eğer öğrenci zaten "Profil Düzenleme" sayfasındaysa veya çıkış yapıyorsa dokunma!
            if ((controllerName == "StudentPanel" && actionName == "EditProfile") ||
                 controllerName == "Account" ||
                 actionName == "Error")
            {
                return; // Polise "İşlem yapma, doğru yerdeler" diyoruz.
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
                    if (student != null && (student.DepartmentId == null || string.IsNullOrEmpty(student.UniversityName)))
                    {
                        // 🔐 5. ADIM: KİLİTLEME VE YÖNLENDİRME
                        // Eğer bilgileri eksikse ve EditProfile dışında bir yerdeyse (Dashboard dahil), oraya şutla!
                        context.Result = new RedirectToActionResult("EditProfile", "StudentPanel", null);
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}