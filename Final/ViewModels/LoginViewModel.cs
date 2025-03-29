using System.ComponentModel.DataAnnotations;

namespace Final.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string UserNameOrEmail { get; set; } // Trường này có thể là Email hoặc UserName

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
