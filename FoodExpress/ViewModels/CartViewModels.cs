namespace FoodExpress.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int FoodItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FoodName { get => Name; set => Name = value; }
        public string? Image { get; set; }
        public string? FoodImage { get => Image; set => Image = value; }
        public decimal Price { get; set; }
        public decimal UnitPrice { get => Price; set => Price = value; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public int? RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public decimal SubTotal => Items.Sum(i => i.SubTotal);
        public decimal DiscountAmount { get; set; } = 0;
        public decimal DeliveryFee { get; set; } = 49.00m;
        public decimal TotalAmount => Math.Max(0, SubTotal - DiscountAmount + (Items.Any() ? DeliveryFee : 0));
        public string? CouponCode { get; set; }
        public string? CouponMessage { get; set; }
        public bool IsCouponValid { get; set; }
        public int TotalItemCount => Items.Sum(i => i.Quantity);
    }
}
