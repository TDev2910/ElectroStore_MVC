using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Final.Models;
using Final.Services;
using Final.Services.Vnpay;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext và Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Đăng ký dịch vụ thanh toán VNPay 
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Cấu hình dịch vụ SMTP Email
builder.Services.AddTransient<EmailService>();

// Đăng ký dịch vụ quản lý đơn hàng
builder.Services.AddScoped<Final.Services.OrderService.OrderService>();

// Cấu hình các dịch vụ MVC
builder.Services.AddControllersWithViews();

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session tồn tại 30 phút
    options.Cookie.HttpOnly = true; // Bảo mật session chỉ dùng HTTP
    options.Cookie.IsEssential = true; // Session quan trọng, không bị xóa
});

var app = builder.Build();

// Cấu hình pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Bổ sung Middleware xử lý Session
app.UseSession();

// Xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

// Cấu hình route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Tạo roles Admin và User nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await CreateRoles(serviceProvider);
}

app.Run();

// Phương thức tạo roles và tài khoản Admin
async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Admin", "User" };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Kiểm tra nếu tài khoản Admin chưa tồn tại
    var adminUser = await userManager.FindByEmailAsync("admin123@gmail.com");
    if (adminUser == null)
    {
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        adminUser = new ApplicationUser()
        {
            UserName = "admin123@gmail.com",
            Email = "admin123@gmail.com",
            FullName = "Administrator"
        };

        // Mã hóa mật khẩu của Admin
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "@Trong29102002");

        var createAdminResult = await userManager.CreateAsync(adminUser);

        if (createAdminResult.Succeeded)
        {
            // Gán quyền Admin cho tài khoản
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            // Log lỗi nếu không tạo được Admin
            foreach (var error in createAdminResult.Errors)
            {
                Console.WriteLine($"Error creating admin: {error.Description}");
            }
        }
    }
}