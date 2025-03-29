using System.ComponentModel.DataAnnotations;
namespace Final.ViewModels;

public class SecondLevelPasswordViewModel
{
    [Required]
    public string Password { get; set; }
}