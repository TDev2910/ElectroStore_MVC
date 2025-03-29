using Final.Models;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Final.ViewModels;

namespace Final.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới truy cập được
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private const string SecurityCode = "2910"; // Mã bảo mật mặc định

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // Trang Dashboard Admin
        public IActionResult Dashboard()
        {
            var totalProducts = _context.Products.Count();
            var totalOrders = _context.Orders.Count();
            var totalRevenue = _context.Orders
                .Where(o => o.Status == "Đã thanh toán" || o.Status == "Đã giao hàng" || o.Status == "Đã nhận hàng")
                .Sum(o => o.TotalPrice);
            var totalUsers = _userManager.Users.Count();

            var websiteViews = new List<int> { 50, 60, 70, 80, 90, 100, 110 }; // Example data

            var mostPurchasedProducts = _context.OrderItems
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new { ProductName = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(g => g.Quantity)
                .Take(10)
                .ToList();

            var dashboardViewModel = new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalUsers = totalUsers,
                WebsiteViews = websiteViews,
                MostPurchasedProducts = mostPurchasedProducts.Select(p => new ProductSalesViewModel
                {
                    ProductName = p.ProductName,
                    Quantity = p.Quantity
                }).ToList()
            };

            return View(dashboardViewModel);
        }

        // Trang quản lý sản phẩm
        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View("~/Views/Admin/Product/Index.cshtml", products);
        }

        // Hiển thị form thêm sản phẩm
        public IActionResult Create()
        {
            return View("~/Views/Admin/Product/Create.cshtml"); // Đường dẫn đúng của view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Views/Admin/Product/Create.cshtml", product);
        }

        // Hiển thị form sửa sản phẩm
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Product/Edit.cshtml", product);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Loại bỏ các ký tự không hợp lệ từ giá trị nhập vào (nếu có dấu phân cách hoặc ký tự không phải số)
                existingProduct.Price = Convert.ToDecimal(product.Price.ToString().Replace(",", "").Replace(".", ""));
                existingProduct.DiscountPrice = product.DiscountPrice.HasValue
                    ? Convert.ToDecimal(product.DiscountPrice.Value.ToString().Replace(",", "").Replace(".", ""))
                    : (decimal?)null;

                // Cập nhật các trường khác
                existingProduct.Name = product.Name;
                existingProduct.Stock = product.Stock;
                existingProduct.TotalStock = product.TotalStock;
                existingProduct.Image = product.Image;
                existingProduct.Description = product.Description;
                existingProduct.Screen = product.Screen;
                existingProduct.OS = product.OS;
                existingProduct.Camera = product.Camera;
                existingProduct.RAM = product.RAM;
                existingProduct.Storage = product.Storage;
                existingProduct.Warranty = product.Warranty;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/Product/Edit.cshtml", product);
        }

        // Xem sản phẩm
        public IActionResult Details(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Product/Details.cshtml", product);
        }

        // Xóa sản phẩm
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Product/Delete.cshtml", product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);  // Xóa sản phẩm
                _context.SaveChanges();  // Lưu thay đổi
                TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";  // Thông báo thành công
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm để xóa.";
            }
            return RedirectToAction("Index");  // Quay lại trang danh sách sản phẩm
        }

        // Hiển thị form nhập mã bảo mật
        public IActionResult EnterSecurityCode()
        {
            return View("~/Views/Admin/UserManagement/EnterSecurityCode.cshtml", new SecurityCodeViewModel());
        }

        // Xử lý mã bảo mật và hiển thị danh sách người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateSecurityCode(SecurityCodeViewModel model)
        {
            string enteredCode = $"{model.Code1}{model.Code2}{model.Code3}{model.Code4}";
            if (enteredCode != SecurityCode)
            {
                TempData["ErrorMessage"] = "Mã code không hợp lệ.";
                return RedirectToAction("EnterSecurityCode");
            }

            return RedirectToAction("UserList");
        }

        // Hiển thị danh sách người dùng
        [HttpGet]
        public async Task<IActionResult> UserList(string searchQuery)
        {
            var users = await _userManager.Users.ToListAsync();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                users = users.Where(u => u.UserName.Contains(searchQuery)).ToList();
            }

            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                userList.Add(new UserViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    IsActive = IsUserActive(user.UserName)
                });
            }

            return View("~/Views/Admin/UserManagement/UserList.cshtml", userList);
        }

        private bool IsUserActive(string userName)
        {
            // Logic để kiểm tra trạng thái hoạt động của người dùng
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == userName);
            if (user != null)
            {
                // Giả sử bạn có một thuộc tính LastActivityDate trong ApplicationUser
                // return (DateTime.Now - user.LastActivityDate).TotalMinutes < 5;
                return true; // Giả sử tất cả người dùng đều đang hoạt động
            }
            return false;
        }

        // Hiển thị danh sách đơn hàng
        public async Task<IActionResult> OrderList(string statusFilter = "")
        {
            var orders = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ToListAsync();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                orders = orders.Where(o => o.Status == statusFilter).ToList();
            }

            return View("~/Views/Admin/OrderServices/OrderList.cshtml", orders);
        }

        // Hiển thị chi tiết đơn hàng
        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/OrderServices/OrderDetails.cshtml", order);
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromForm] UpdateOrderStatusModel model)
        {
            var order = await _context.Orders.FindAsync(model.OrderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = model.Status;
            await _context.SaveChangesAsync();

            // Gửi email thông báo cập nhật trạng thái đơn hàng
            var emailBody = GenerateOrderStatusUpdateEmailBody(order);
            await _emailService.SendEmailAsync(order.Email, "Cập nhật trạng thái đơn hàng", emailBody);

            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction("OrderList");
        }

        private string GenerateOrderStatusUpdateEmailBody(Order order)
        {
            var body = new StringBuilder();
            body.AppendLine("<h2>Cập nhật trạng thái đơn hàng</h2>");
            body.AppendLine($"<p><strong>Mã đơn hàng:</strong> {order.Id}</p>");
            body.AppendLine($"<p><strong>Trạng thái mới:</strong> {order.Status}</p>");
            body.AppendLine("<h4>Thông tin đơn hàng</h4>");
            body.AppendLine($"<p><strong>Họ:</strong> {order.FirstName}</p>");
            body.AppendLine($"<p><strong>Tên:</strong> {order.LastName}</p>");
            body.AppendLine($"<p><strong>Số điện thoại:</strong> {order.Phone}</p>");
            body.AppendLine($"<p><strong>Email:</strong> {order.Email}</p>");
            body.AppendLine($"<p><strong>Địa chỉ:</strong> {order.Address}</p>");
            body.AppendLine($"<p><strong>Phương thức vận chuyển:</strong> {order.ShippingMethod}</p>");
            body.AppendLine($"<p><strong>Phương thức thanh toán:</strong> {order.PaymentMethod}</p>");
            body.AppendLine($"<p><strong>Tổng giá trị:</strong> {order.TotalPrice}</p>");
            body.AppendLine("<h4>Sản phẩm trong đơn hàng</h4>");
            body.AppendLine("<ul>");
            foreach (var item in order.OrderItems)
            {
                body.AppendLine($"<li>{item.Product.Name} - Số lượng: {item.Quantity} - Giá: {item.Price}</li>");
            }
            body.AppendLine("</ul>");
            return body.ToString();
        }

        // xóa đơn hàng
        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("OrderDetails", new { id });
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();

            TempData["Success"] = "Đơn hàng đã được xóa thành công!";
            return Redirect("/Admin/OrderList");
        }
        // Hiển thị danh sách phản hồi
        public async Task<IActionResult> FeedbackList()
        {
            var feedbacks = await _context.UserFeedbacks.ToListAsync();
            return View("~/Views/Admin/ContactFeedback/FeedbackList.cshtml", feedbacks);
        }
        public async Task<IActionResult> FeedbackDetails(int id)
        {
            var feedback = await _context.UserFeedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/ContactFeedback/FeedbackDetails.cshtml", feedback);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondFeedback(int id, string response)
        {
            var feedback = await _context.UserFeedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            feedback.AdminResponse = response;
            feedback.RespondedAt = DateTime.Now;
            feedback.Status = "Đã trả lời"; // Update status
            await _context.SaveChangesAsync();

            // Gửi email phản hồi đến người dùng
            var emailBody = $"<p>Phản hồi của bạn: {feedback.Message}</p><p>Phản hồi từ admin: {response}</p>";
            await _emailService.SendEmailAsync(feedback.Email, "Phản hồi từ Admin", emailBody);

            TempData["SuccessMessage"] = "Phản hồi đã được gửi thành công!";
            return RedirectToAction("FeedbackList");
        }
        
        //Update trang thái phản hồi
        [HttpPost]
        public async Task<IActionResult> UpdateFeedbackStatus([FromForm] UpdateFeedbackStatusModel model)
        {
            var feedback = await _context.UserFeedbacks.FindAsync(model.FeedbackId);
            if (feedback == null)
            {
                return NotFound();
            }

            feedback.Status = model.Status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật trạng thái phản hồi thành công!";
            return RedirectToAction("FeedbackList");
        }
    }
    
    public class UserViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class SecurityCodeViewModel
    {
        [Required]
        public string Code1 { get; set; }
        [Required]
        public string Code2 { get; set; }
        [Required]
        public string Code3 { get; set; }
        [Required]
        public string Code4 { get; set; }
    }

    public class UpdateOrderStatusModel
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
    }
}