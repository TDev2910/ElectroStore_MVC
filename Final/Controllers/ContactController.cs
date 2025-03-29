using Final.Models;
using Final.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Final.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public ContactController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactFormModel model)
        {
            if (ModelState.IsValid)
            {
                var feedback = new UserFeedback
                {
                    Name = model.Name,
                    Email = model.Email,
                    Message = model.Message,
                    CreatedAt = DateTime.Now,
                    AdminResponse = string.Empty // Đảm bảo AdminResponse được thiết lập
                };

                _context.UserFeedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                // Gửi email phản hồi đến người dùng
                var emailBody = $"<p>Cảm ơn bạn đã liên hệ với chúng tôi. Đây là nội dung phản hồi của bạn: {model.Message}</p>";
                await _emailService.SendEmailAsync(model.Email, "Xác nhận phản hồi", emailBody);

                ViewBag.Message = "Phản hồi của bạn đã được gửi thành công!";
                return View();
            }

            return View(model);
        }
    }
}