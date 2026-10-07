using FoodExpress.Models;

namespace FoodExpress.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalRestaurants { get; set; }
        public int TotalFoodItems { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<FoodItem> TopSellingFoods { get; set; } = new List<FoodItem>();
    }

    public class OwnerDashboardViewModel
    {
        public int TotalRestaurants { get; set; }
        public int TotalFoodItems { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }

    public class ReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrdersCount { get; set; }
        public int CompletedOrdersCount { get; set; }
        public int CancelledOrdersCount { get; set; }
        public decimal AverageOrderValue { get; set; }

        public List<TopFoodReportItem> TopFoodItems { get; set; } = new List<TopFoodReportItem>();
        public List<RestaurantSalesReportItem> RestaurantSales { get; set; } = new List<RestaurantSalesReportItem>();
        public List<DailySalesReportItem> DailySales { get; set; } = new List<DailySalesReportItem>();
    }

    public class TopFoodReportItem
    {
        public string FoodName { get; set; } = string.Empty;
        public string RestaurantName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RestaurantSalesReportItem
    {
        public string RestaurantName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailySalesReportItem
    {
        public string Date { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
