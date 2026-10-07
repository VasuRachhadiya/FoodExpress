using FoodExpress.Models;

namespace FoodExpress.ViewModels
{
    public class OrderListViewModel
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string? RestaurantImage { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int TotalItems { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }

    public class OrderDetailViewModel
    {
        public Order Order { get; set; } = null!;
        public bool CanCancel => Order.OrderStatus == OrderStatus.Pending;
        public int StepIndex => Order.OrderStatus switch
        {
            OrderStatus.Pending => 1,
            OrderStatus.Confirmed => 2,
            OrderStatus.Preparing => 3,
            OrderStatus.OutForDelivery => 4,
            OrderStatus.Delivered => 5,
            _ => 0
        };
    }
}
