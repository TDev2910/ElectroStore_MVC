using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Final.Models;
using Final.Services;
using Final.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Final.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, EmailService emailService)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        //Trang chủ
        public IActionResult Index()
        {
            return View();
        }

        //Đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = null;

                // Kiểm tra tài khoản bằng email hoặc username
                if (!string.IsNullOrEmpty(model.UserNameOrEmail))
                {
                    user = model.UserNameOrEmail.Contains("@")
                        ? await _userManager.FindByEmailAsync(model.UserNameOrEmail)
                        : await _userManager.FindByNameAsync(model.UserNameOrEmail);
                }

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin"); // 👉 Nếu là Admin, điều hướng đến Dashboard Admin
                        }
                        else if (roles.Contains("User"))
                        {
                            return RedirectToAction("Product", "User"); // 👉 Nếu là User, điều hướng đến Dashboard User
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Đăng nhập không hợp lệ.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Người dùng không tồn tại.");
                }
            }
            return View(model);
        }

        //Đăng ký tài khoản
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!model.Password.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu phải có ít nhất một ký tự không phải chữ và số.");
                    return View(model);
                }

                if (!model.Password.Any(ch => char.IsDigit(ch)))
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu phải có ít nhất một chữ số ('0'-'9').");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User"); // Assign default User role
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard", "User"); // Redirect to User Dashboard after registration
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        //Đăng xuất
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //Quên mật khẩu
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var resetCode = new Random().Next(100000, 999999).ToString(); //Tạo mã OTP ngẫu nhiên

                // 🔹 Kiểm tra xem Session đã được thiết lập chưa
                HttpContext.Session.SetString("ResetCode", resetCode);
                HttpContext.Session.SetString("ResetEmail", model.Email);

                var subject = "Mã xác nhận đặt lại mật khẩu";
                var body = $"<p>Xin chào,</p><p>Mã xác nhận của bạn là: <strong>{resetCode}</strong></p><p>Vui lòng nhập mã này để tiếp tục đặt lại mật khẩu.</p>";

                await _emailService.SendEmailAsync(model.Email, subject, body);

                return RedirectToAction("VerifyResetCode");
            }

            return View(model);
        }

        public IActionResult VerifyResetCode()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyResetCodePost()
        {
            string enteredCode = string.Concat(
                Request.Form["code1"], Request.Form["code2"], Request.Form["code3"],
                Request.Form["code4"], Request.Form["code5"], Request.Form["code6"]
            ); // Gộp 6 số lại thành mã OTP hoàn chỉnh

            var storedCode = HttpContext.Session.GetString("ResetCode");
            var email = HttpContext.Session.GetString("ResetEmail");

            if (storedCode == enteredCode)
            {
                HttpContext.Session.SetString("VerifiedEmail", email);
                return RedirectToAction("ResetPassword"); // Chuyển đến trang đổi mật khẩu
            }

            ViewBag.Error = "Mã OTP không đúng! Vui lòng nhập lại.";
            return View("VerifyResetCode");
        }


        [HttpPost]
        public async Task<IActionResult> ResendOTP() //phương thức xử lý yêu cầu gửi lại mã OTP
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Không tìm thấy email để gửi lại mã OTP." });
            }

            // Tạo mã OTP mới
            var resetCode = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("ResetCode", resetCode);

            var subject = "Mã xác nhận đặt lại mật khẩu (OTP)";
            var body = $"<p>Xin chào,</p><p>Mã xác nhận mới của bạn là: <strong>{resetCode}</strong></p><p>Vui lòng nhập mã này để tiếp tục đặt lại mật khẩu.</p>";

            await _emailService.SendEmailAsync(email, subject, body);

            return Json(new { success = true, message = "Mã OTP mới đã được gửi đến email của bạn." });
        }

        public IActionResult ResetPassword()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("VerifiedEmail")))
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string Password, string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("VerifiedEmail")))
            {
                return RedirectToAction("ForgotPassword");
            }

            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var email = HttpContext.Session.GetString("VerifiedEmail");
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetToken, Password);

            if (resetPassResult.Succeeded)
            {
                return Content("success"); // Gửi phản hồi thành công cho JavaScript
            }

            foreach (var error in resetPassResult.Errors)
            {
                ViewBag.Error += error.Description + " ";
            }
            return View();
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //login with google
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Home");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider); // Redirect to Google for login
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Product", "User");
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    createResult = await _userManager.AddLoginAsync(user, info);
                    if (createResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Product", "User");
                    }
                }

                return RedirectToAction("Login");
            }
        }

    }
}