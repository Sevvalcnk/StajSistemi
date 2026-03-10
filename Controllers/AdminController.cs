using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin girebilir
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}