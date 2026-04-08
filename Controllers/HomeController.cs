using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;

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
        // 1. Veritabanından ham verileri çekiyoruz
        var applications = await _unitOfWork.InternshipApplications.GetAllAsync();
        var allInternships = await _unitOfWork.Internships.GetAllIncludingAsync(i => i.Department, i => i.City);

        // ✅ BAŞVURU BİLDİRİMİ: Beklemede olanları sayıp Layout'a gönderiyoruz
        ViewBag.NewApplicationsCount = applications.Count(a => a.Status == "Beklemede" && !a.IsDeleted);

        // Dashboard kartları için aktif ilan sayısını mühürle
        ViewBag.ActiveInternshipsCount = allInternships.Count(i => i.Status == "Aktif" && !i.IsDeleted);

        // 🔥 YENİ KRİTİK MÜHÜR: Mesaj Bildirimi
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            var currentUserId = int.Parse(userId);
            var allMessages = await _unitOfWork.ChatMessages.GetAllAsync();
            if (allMessages != null)
            {
                ViewBag.UnreadMessagesCount = allMessages
                    .Count(m => m.ReceiverId == currentUserId && !m.IsRead);
            }
        }

        // ✅ ANA SAYFA İLANLARI (CAROUSEL İÇİN GÜNCELLENDİ): 
        // Eskiden .Take(3) idi, kaydırma olması için sınırı 15'e çıkardık.
        // Artık 3'ten fazla ilan olduğunda oklar otomatik belirecek!
        var homeInternships = allInternships
            .Where(i => i.Status == "Aktif" && !i.IsDeleted)
            .OrderByDescending(i => i.Id)
            .Take(15)
            .ToList();

        // Listeyi View tarafına model olarak gönderiyoruz
        return View(homeInternships);
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