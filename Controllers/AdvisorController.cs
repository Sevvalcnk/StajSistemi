using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "Advisor")] // Sadece Danışman girebilir
    public class AdvisorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}