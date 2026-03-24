using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract; // ✅ KRİTİK: Kırmızı çizginin ilacı burası!

namespace StajSistemi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        // Veritabanından verileri çekiyoruz
        var applications = await _unitOfWork.InternshipApplications.GetAllAsync();
        var internships = await _unitOfWork.Internships.GetAllAsync();

        // ✅ BİLDİRİM BALONCUĞU: Beklemede olanları sayıp Layout'a gönderiyoruz
        ViewBag.NewApplicationsCount = applications.Count(a => a.Status == "Beklemede" && !a.IsDeleted);

        // Dashboard kartları için
        ViewBag.ActiveInternshipsCount = internships.Count(i => i.Status == "Aktif" && !i.IsDeleted);

        return View();
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}