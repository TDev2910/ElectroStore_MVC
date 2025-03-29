using Microsoft.AspNetCore.Identity;

namespace Final.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Các thuộc tính tùy chỉnh mà bạn muốn thêm vào người dùng
        public string FullName { get; set; }  // Tên đầy đủ
    }
}
