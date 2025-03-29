using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Final.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; } // ID sản phẩm
        [ForeignKey("ProductId")]
        public Product Product { get; set; } // Thông tin sản phẩm

        public int Quantity { get; set; } // Số lượng sản phẩm
        public decimal Price { get; set; } // Giá sản phẩm
    }
}