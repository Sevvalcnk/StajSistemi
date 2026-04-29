using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.data; // 🛡️ MÜHÜR: Context için ekledik
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;

namespace StajSistemi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context; // 🛡️ LİYAKATLİ EKLEME

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _context = context; // 🛡️ Mühürlendi
    }

    public async Task<IActionResult> Index()
    {
        // 1. Sayaçlar için verileri çekiyoruz (Mevcut mantığın korundu)
        var applications = await _unitOfWork.InternshipApplications.GetAllAsync();

        // 2. ✅ KRİTİK MÜHÜR: Bölüm isimlerini derinlemesine getiren liyakatli sorgu!
        // Repository'deki hata veren Include yerine doğrudan Context ile en sağlam yolu kullanıyoruz.
        var allInternships = await _context.Internships
            .Include(i => i.InternshipDepartments)
                .ThenInclude(id => id.Department) // 🚀 İşte isimlerin geldiği o altın halka!
            .Include(i => i.City)
            .Where(i => !i.IsDeleted && i.Status == ApplicationStatus.Active)
            .OrderByDescending(i => i.Id)
            .Take(15)
            .ToListAsync();

        // ✅ BİLDİRİM SAYAÇLARI (Aynen Korundu)
        ViewBag.NewApplicationsCount = applications.Count(a => a.Status == ApplicationStatus.Pending && !a.IsDeleted);
        ViewBag.ActiveInternshipsCount = allInternships.Count;

        // 🔥 MESAJ BİLDİRİMİ (Aynen Korundu)
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

        // Listeyi View tarafına model olarak gönderiyoruz
        return View(allInternships);
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