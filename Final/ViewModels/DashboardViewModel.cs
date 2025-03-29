namespace Final.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalUsers { get; set; }
    public List<int> WebsiteViews { get; set; }
    public List<ProductSalesViewModel> MostPurchasedProducts { get; set; }
    public List<string> RevenueLabels { get; set; } 
    public List<decimal> RevenueData { get; set; } 
}

public class ProductSalesViewModel
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
}