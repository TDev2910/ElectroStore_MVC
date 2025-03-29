using System.ComponentModel.DataAnnotations;

namespace Final.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        // Tên người dùng (Username)
        [Required(ErrorMessage = "Tên người dùng không được để trống!")]
        [StringLength(100, ErrorMessage = "Tên người dùng không thể dài hơn 100 ký tự.")]
        public string UserName { get; set; }

        // Tên đầy đủ của người dùng
        [Required(ErrorMessage = "Họ và tên không được để trống!")]
        [StringLength(100, ErrorMessage = "Họ và tên không thể dài hơn 100 ký tự.")]
        public string FullName { get; set; }
    }
}