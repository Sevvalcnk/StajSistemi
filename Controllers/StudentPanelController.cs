using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Student")] // Sadece Öğrenci girebilir
    public class StudentPanelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}