using System.Collections.Generic;
using System.Linq;
using Final.Models; 

namespace Final.Models
{
    public class Cart
    {
        private List<CartItem> ItemCollection = new List<CartItem>();

        public virtual IEnumerable<CartItem> Items => ItemCollection;

        // Thêm sản phẩm vào giỏ hàng
        public virtual void AddItem(Product product, int quantity)
        {
            CartItem item = ItemCollection.FirstOrDefault(p => p.Product.Id == product.Id);
            if (item == null)
            {
                ItemCollection.Add(new CartItem()
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = quantity,
                    Price = product.Price  // Đảm bảo gán giá trị đúng
                });
            }
            else
            {
                item.Quantity += quantity;
            }
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public virtual void RemoveItem(int productId)
        {
            ItemCollection.RemoveAll(p => p.Product.Id == productId);
        }

        // Giảm số lượng sản phẩm
        public virtual void DecreaseQuantity(int productId)
        {
            var item = ItemCollection.FirstOrDefault(p => p.Product.Id == productId);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    RemoveItem(productId);
                }
            }
        }

        // Xóa toàn bộ giỏ hàng
        public virtual void Clear() => ItemCollection.Clear();

        // Tính tổng số lượng sản phẩm trong giỏ hàng
        public int TotalItems() => ItemCollection.Sum(i => i.Quantity);

        // Tính tổng tiền trong giỏ hàng
        public decimal TotalPrice() => ItemCollection.Sum(i => i.Product.Price * i.Quantity);
    }
}


//namespace Final.Models
//{
//    public class Cart
//    {
//        private List<CartItem> ItemCollection = new List<CartItem>();

//        public virtual IEnumerable<CartItem> Items => ItemCollection;

//        // Thêm sản phẩm vào giỏ hàng (cộng dồn nếu đã có)
//        public virtual void AddItem(Product product, int quantity)
//        {
//            CartItem item = ItemCollection.FirstOrDefault(p => p.Product.Id == product.Id);
//            if (item == null)
//            {
//                ItemCollection.Add(new CartItem()
//                {
//                    Product = product,
//                    Quantity = quantity
//                });
//            }
//            else
//            {
//                item.Quantity += quantity;
//            }

//        }

//        // Xóa sản phẩm khỏi giỏ hàng (xóa hoàn toàn)
//        public virtual void RemoveItem(int productId)
//        {
//            ItemCollection.RemoveAll(p => p.Product.Id == productId);
//        }

//        // Giảm số lượng sản phẩm (nếu số lượng = 1 thì xóa)
//        public virtual void DecreaseQuantity(int productId)
//        {
//            var item = ItemCollection.FirstOrDefault(p => p.Product.Id == productId);
//            if (item != null)
//            {
//                if (item.Quantity > 1)
//                {
//                    item.Quantity--;
//                }
//                else
//                {
//                    RemoveItem(productId);
//                }
//            }
//        }

//        // Xóa toàn bộ giỏ hàng
//        public virtual void Clear() => ItemCollection.Clear();

//        // Tính tổng số lượng sản phẩm trong giỏ hàng
//        public int TotalItems() => ItemCollection.Sum(i => i.Quantity);

//        // Tính tổng tiền trong giỏ hàng
//        public decimal TotalPrice() => ItemCollection.Sum(i => i.Product.Price * i.Quantity);
//    }

//    public class CartItem
//    {
//        public int CartItemID { get; set; }
//        public Product Product { get; set; }
//        public int Quantity { get; set; } // Đã sửa lỗi chính tả "Quanlity" -> "Quantity"
//    }
//}

