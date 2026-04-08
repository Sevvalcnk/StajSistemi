using Microsoft.AspNetCore.Mvc;
using StajSistemi.Repositories.Abstract;
using System.Security.Claims;

namespace StajSistemi.Components
{
    // Bu sınıf, her sayfa yüklendiğinde otomatik çalışır
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public SidebarViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Giriş yapan kullanıcının ID'sini Claims üzerinden alıyoruz
            var userIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int unreadCount = 0;

            if (!string.IsNullOrEmpty(userIdString))
            {
                int userId = int.Parse(userIdString);
                // Veritabanına gidip "Bana gelen okunmamış mesajları say" diyoruz
                var allMessages = await _unitOfWork.ChatMessages.GetAllAsync();
                unreadCount = allMessages.Count(m => m.ReceiverId == userId && !m.IsRead);
            }

            // Sayıyı View tarafına "UnreadMessagesCount" adıyla gönderiyoruz
            ViewBag.UnreadMessagesCount = unreadCount;
            return View();
        }
    }
}