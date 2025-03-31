using Final.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Final.Controllers
{
    [Authorize(Roles = "User")] // Chỉ User truy cập được
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hiển thị danh sách sản phẩm trên trang Dashboard
        [Authorize(Roles = "User")]
        public IActionResult Dashboard()
        {
            return View();
        }

        // Hiển thị product - trang chủ với lọc và tìm kiếm
        public IActionResult Product(string searchTerm = null, string priceRange = null, string os = null)
        {
            // Lấy danh sách hệ điều hành từ database
            ViewBag.OSOptions = _context.Products.Select(p => p.OS).Distinct().ToList();
            ViewBag.SelectedOS = os;
            ViewBag.SelectedPriceRange = priceRange;
            ViewBag.SearchTerm = searchTerm;

            // Lấy danh sách sản phẩm
            var products = _context.Products.AsQueryable();

            // Lọc theo hệ điều hành nếu có
            if (!string.IsNullOrEmpty(os))
            {
                products = products.Where(p => p.OS.ToLower() == os.ToLower());
            }

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "10-15":
                        products = products.Where(p => p.Price >= 10000000 && p.Price <= 15000000);
                        break;
                    case "15-30":
                        products = products.Where(p => p.Price >= 15000000 && p.Price <= 30000000);
                        break;
                    case "30-50":
                        products = products.Where(p => p.Price >= 30000000 && p.Price <= 50000000);
                        break;
                }
            }

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            // Trả về danh sách sản phẩm sau khi lọc
            return View(products.ToList());
        }

        // Đổi mật khẩu
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
                return RedirectToAction("Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }


        // Xem đơn hàng
        public async Task<IActionResult> Orders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = user.Email; // Lấy email từ AspNetUsers
            var orders = await _context.Orders
                .Where(o => o.Email.ToLower() == userEmail.ToLower())
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                ViewData["Message"] = "Bạn chưa có đơn hàng nào.";
            }
            else
            {
                ViewData["Message"] = null;
            }

            return View(orders);
        }

        // Đặt đơn hàng
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            order.Email = user.Email;
            order.OrderDate = DateTime.Now;
            order.Status = "Pending"; // Trạng thái đơn hàng ban đầu

            // Giả sử OrderItems đã được điền từ phía client (form hoặc giỏ hàng)
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                ModelState.AddModelError("", "Đơn hàng phải chứa ít nhất một sản phẩm.");
                return View(order); // Trả về view đặt hàng nếu có lỗi
            }

            // Tính tổng giá
            order.TotalPrice = order.OrderItems.Sum(oi => oi.Price * oi.Quantity);

            // Lưu đơn hàng
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đơn hàng của bạn đã được đặt thành công.";
            return RedirectToAction("Orders");
        }

        // Cập nhật thông tin người dùng
        public IActionResult UpdateProfile()
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
                return RedirectToAction("Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        
        public async Task<IActionResult> RequestCancelOrder(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Email.ToLower() == user.Email.ToLower());

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Cancelled" || order.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Đơn hàng không thể yêu cầu hủy vì đã được xử lý.";
                return RedirectToAction("Orders");
            }

            order.Status = "Yêu cầu hủy đơn hàng";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yêu cầu hủy đơn hàng đã được gửi.";
            return RedirectToAction("Orders");
        }

        // Hiển thị trang tài khoản
        public IActionResult Account()
        {
            return View();
        }
    }
}