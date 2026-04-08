using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StajSistemi.DTOs;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using System.Security.Claims;
// ✅ Sinyal Mühürü İçin Gerekli Kütüphaneler
using Microsoft.AspNetCore.SignalR;
using StajSistemi.Hubs;

namespace StajSistemi.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        // ✅ CANLI YAYIN ARACI (SignalR Hub Context)
        private readonly IHubContext<ChatHub> _hubContext;

        public StudentController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _hubContext = hubContext; // Mühürlendi!
        }

        [Authorize(Roles = "Advisor,Admin")]
        public async Task<IActionResult> Index()
        {
            var students = await _unitOfWork.Students.GetAllIncludingAsync(s => s.Department);
            var activeStudents = students.Where(s => !s.IsDeleted).ToList();
            await SetNotificationCount();
            var studentDtos = _mapper.Map<List<StudentDto>>(activeStudents);
            return View(studentDtos);
        }

        [Authorize(Roles = "Advisor,Admin")]
        public async Task<IActionResult> Inbox()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var currentUserId = int.Parse(userIdClaim);
            var messages = await _unitOfWork.ChatMessages.GetAllIncludingAsync(m => m.Sender);
            var inboxMessages = messages
                .Where(m => m.ReceiverId == currentUserId)
                .GroupBy(m => m.SenderId)
                .Select(g => g.OrderByDescending(m => m.SentDate).First())
                .OrderByDescending(m => m.SentDate)
                .ToList();
            await SetNotificationCount();
            return View(inboxMessages);
        }

        [Authorize(Roles = "Advisor,Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null || student.IsDeleted) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var currentUserId = int.Parse(userIdClaim);
            var chatHistory = await _unitOfWork.ChatMessages.GetAllIncludingAsync(m => m.Sender);
            var messages = chatHistory
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == id) ||
                            (m.SenderId == id && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentDate)
                .ToList();

            // ✅ MÜHÜR: Okunmayan mesajları "Okundu" yap
            var unreadMessages = messages
                .Where(m => m.ReceiverId == currentUserId && m.SenderId == id && !m.IsRead)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var m in unreadMessages) { m.IsRead = true; }
                await _unitOfWork.SaveAsync();

                // 🚀 CANLI SİNYAL: Karşı tarafa (mesajı gönderen kişiye) mesajın okunduğunu haber ver!
                await _hubContext.Clients.User(id.ToString()).SendAsync("ReceiveReadReceipt", currentUserId);
            }

            await SetNotificationCount();
            ViewBag.ChatHistory = messages;
            var internships = await _unitOfWork.Internships.GetAllAsync();
            ViewBag.ActiveInternships = internships.Where(i => !i.IsDeleted).ToList();
            var studentDto = _mapper.Map<StudentDto>(student);
            return View(studentDto);
        }

        [Authorize(Roles = "Advisor,Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await SetNotificationCount();
            var model = new StudentDto();
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentsList = new SelectList(departments ?? new List<Department>(), "Id", "DepartmentName");
            return View(model);
        }

        [Authorize(Roles = "Advisor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentDto studentDto)
        {
            if (ModelState.IsValid)
            {
                var student = _mapper.Map<AppUser>(studentDto);
                student.UserName = studentDto.StudentNo ?? studentDto.Email;
                student.IsDeleted = false;
                student.City = null;
                student.Department = null;

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(currentUserId)) { student.AdvisorId = int.Parse(currentUserId); }

                var result = await _userManager.CreateAsync(student, "Ogrenci123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(student, "Student");
                    TempData["SuccessMessage"] = "Öğrenci başarıyla mühürlendi! 🥂";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors) { ModelState.AddModelError("", error.Description); }
            }
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentsList = new SelectList(departments ?? new List<Department>(), "Id", "DepartmentName");
            await SetNotificationCount();
            return View(studentDto);
        }

        [Authorize(Roles = "Advisor,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null || student.IsDeleted) return NotFound();
            await SetNotificationCount();
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentsList = new SelectList(departments ?? new List<Department>(), "Id", "DepartmentName");
            var studentDto = _mapper.Map<StudentDto>(student);
            return View(studentDto);
        }

        [Authorize(Roles = "Advisor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentDto studentDto)
        {
            if (ModelState.IsValid)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentDto.Id);
                if (student == null) return NotFound();
                _mapper.Map(studentDto, student);
                student.UserName = studentDto.StudentNo ?? studentDto.Email;
                student.City = null;
                student.Department = null;

                _unitOfWork.Students.Update(student);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "Bilgiler güncellendi! 🛡️";
                return RedirectToAction(nameof(Index));
            }
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentsList = new SelectList(departments ?? new List<Department>(), "Id", "DepartmentName");
            await SetNotificationCount();
            return View(studentDto);
        }

        [Authorize(Roles = "Advisor,Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null) return NotFound();
            student.IsDeleted = true;
            await _unitOfWork.SaveAsync();
            TempData["SuccessMessage"] = "Arşive kaldırıldı. 🛡️";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int ReceiverId, string Content)
        {
            if (string.IsNullOrEmpty(Content)) return Redirect(Request.Headers["Referer"].ToString());
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var currentUserId = int.Parse(userIdClaim);
            var message = new ChatMessage
            {
                SenderId = currentUserId,
                ReceiverId = ReceiverId,
                Content = Content,
                SentDate = DateTime.Now,
                IsRead = false
            };
            await _unitOfWork.ChatMessages.AddAsync(message);
            await _unitOfWork.SaveAsync();

            // 🚀 CANLI SİNYAL: Alıcıya yeni mesaj geldiğini anında haber ver!
            await _hubContext.Clients.User(ReceiverId.ToString()).SendAsync("ReceiveNewMessage", currentUserId, Content);

            TempData["SuccessMessage"] = "Mesajın başarıyla uçuruldu! 🥂";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _unitOfWork.ChatMessages.GetByIdAsync(id);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || message == null)
                return Json(new { success = false, message = "Mesaj bulunamadı kardaşım!" });

            var currentUserId = int.Parse(userIdClaim);

            if (message.SenderId == currentUserId || message.ReceiverId == currentUserId)
            {
                _unitOfWork.ChatMessages.Delete(message);
                await _unitOfWork.SaveAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Bu mesajı silme yetkiniz yok kardaşım! 🛡️" });
        }

        private async Task SetNotificationCount()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return;
            var currentUserId = int.Parse(userIdClaim);

            var messages = await _unitOfWork.ChatMessages.GetAllAsync();
            if (messages != null)
            {
                ViewBag.UnreadMessagesCount = messages.Count(m => m.ReceiverId == currentUserId && !m.IsRead);
            }
        }
    }
}