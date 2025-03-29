using System.ComponentModel.DataAnnotations;

namespace Final.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Chỉ để một Range cho Price
        public decimal Price { get; set; }

        // Nếu có giá giảm, phải nhỏ hơn hoặc bằng giá gốc
        public decimal? DiscountPrice { get; set; }

        public int Stock { get; set; }

        public int TotalStock { get; set; }
        public string Image { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Screen { get; set; } = string.Empty;
        public string OS { get; set; } = string.Empty;
        public string Camera { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string Warranty { get; set; } = string.Empty;



        // Tính phần trăm giảm giá nếu có
        public decimal? DiscountPercentage
        {
            get
            {
                if (DiscountPrice.HasValue && DiscountPrice < Price)
                {
                    return Math.Round(100 - (DiscountPrice.Value / Price * 100), 1);
                }
                return null;
            }
        }
    }
}