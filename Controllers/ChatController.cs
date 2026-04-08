using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using StajSistemi.Hubs;
using System.Security.Claims;

namespace StajSistemi.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // ✅ GÜÇLENDİRİLMİŞ MÜHÜR: Mesajlaşma Paneli Ana Metodu
        public async Task<IActionResult> Index(int? receiverId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            // View tarafında sağ-sol ayrımı için ID'yi mühürle
            ViewBag.CurrentUserId = currentUser.Id;

            // Tüm mesajları çek (Include ile ilişkileri bağla)
            var allMyMessages = await _unitOfWork.ChatMessages.GetAllIncludingAsync(m => m.Sender, m => m.Receiver);

            // 1. SOL TARAF: Son görüşülen kişiler listesi
            var recentContacts = allMyMessages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .Select(m => m.SenderId == currentUser.Id ? m.Receiver : m.Sender)
                .Where(u => u != null && u.Id != currentUser.Id)
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();

            if (receiverId.HasValue && !recentContacts.Any(c => c.Id == receiverId.Value))
            {
                var newContact = await _userManager.FindByIdAsync(receiverId.Value.ToString());
                if (newContact != null) recentContacts.Insert(0, newContact);
            }
            ViewBag.RecentContacts = recentContacts;

            // 2. SAĞ TARAF: Aktif Sohbet ve Okundu İşlemi
            if (receiverId.HasValue)
            {
                var receiver = await _userManager.FindByIdAsync(receiverId.Value.ToString());
                if (receiver != null)
                {
                    var chatHistory = allMyMessages
                        .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == receiverId) ||
                                    (m.SenderId == receiverId && m.ReceiverId == currentUser.Id))
                        .OrderBy(m => m.SentDate)
                        .ToList();

                    // ✅ OKUNDU MANTIGI: Sadece bana gelen ve okunmamış olanları "Okundu" yap
                    var incomingUnreadMessages = chatHistory
                        .Where(m => m.ReceiverId == currentUser.Id && !m.IsRead).ToList();

                    if (incomingUnreadMessages.Any())
                    {
                        foreach (var m in incomingUnreadMessages) { m.IsRead = true; }
                        await _unitOfWork.SaveAsync();

                        // 🛰️ CANLI SİNYAL: Mesajı gönderen tarafa "Okundu (Mavi Tik)" bilgisini uçur
                        await _hubContext.Clients.User(receiverId.Value.ToString()).SendAsync("ReceiveReadReceipt", currentUser.Id);
                    }

                    ViewBag.Receiver = receiver;

                    // 🔥 GLOBAL SAYAÇ MÜHÜRÜ: Okundu işleminden SONRA hesaplıyoruz ki sayı düşsün
                    var loggedUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    ViewBag.UnreadMessagesCount = (await _unitOfWork.ChatMessages.GetAllAsync())
                        .Count(m => m.ReceiverId == loggedUserId && !m.IsRead);

                    return View(chatHistory);
                }
            }

            // Eğer sohbet seçili değilse toplam okunmamış sayısını yine de hesapla
            var userIdForBadge = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.UnreadMessagesCount = (await _unitOfWork.ChatMessages.GetAllAsync())
                .Count(m => m.ReceiverId == userIdForBadge && !m.IsRead);

            return View(new List<ChatMessage>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int receiverId, string content, int? suggestedInternshipId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null || string.IsNullOrWhiteSpace(content) || receiverId <= 0)
            {
                TempData["ErrorMessage"] = "Mesaj boş mühürlenemez! 🛡️";
                return RedirectToAction("Index", new { receiverId = receiverId });
            }

            var message = new ChatMessage
            {
                SenderId = currentUser.Id,
                ReceiverId = receiverId,
                Content = content.Trim(),
                SentDate = DateTime.Now,
                SuggestedInternshipId = suggestedInternshipId,
                IsRead = false
            };

            await _unitOfWork.ChatMessages.AddAsync(message);
            await _unitOfWork.SaveAsync();

            // 🛰️ CANLI SİNYAL: Alıcıya "Yeni Mesaj Geldi!" bilgisini uçur
            await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveNewMessage", currentUser.Id, content);

            return RedirectToAction("Index", new { receiverId = receiverId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _unitOfWork.ChatMessages.GetByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(User);

            if (message == null || currentUser == null)
                return Json(new { success = false, message = "Mesaj bulunamadı kardaşım!" });

            if (message.SenderId == currentUser.Id || message.ReceiverId == currentUser.Id)
            {
                _unitOfWork.ChatMessages.Delete(message);
                await _unitOfWork.SaveAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Bu mesajı silmeye yetkin yok! 🛡️" });
        }
    }
}