using Final.Models;
using System.ComponentModel.DataAnnotations;

public class OrderModel
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Phone { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Address { get; set; }
    [Required]
    public string ShippingMethod { get; set; }  // Giao hàng tận nơi hoặc hẹn lấy tại cửa hàng
    public string City { get; set; }
    public string Store { get; set; }
    public string Notes { get; set; }
    [Required]
    public string PaymentMethod { get; set; }
    public string Vnp_TmnCode { get; set; }
    public string Vnp_HashSecret { get; set; }
    public string Vnp_Url
    {
        get; set;
    }
    public decimal TotalPrice
    {
        get { return CartItems?.Sum(i => i.Price * i.Quantity) ?? 0; }
    }

    // ✅ Đảm bảo CartItems có kiểu dữ liệu đúng
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();

    // ✅ THÊM THUỘC TÍNH `Province` nếu bạn có sử dụng trong Checkout
    public string Province { get; set; }

}