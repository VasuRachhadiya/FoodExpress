using FoodExpress.Models;

namespace FoodExpress.ViewModels
{
    public class HomeViewModel
    {
        public List<FoodCategory> Categories { get; set; } = new List<FoodCategory>();
        public List<Restaurant> FeaturedRestaurants { get; set; } = new List<Restaurant>();
        public List<FoodItem> PopularFoodItems { get; set; } = new List<FoodItem>();
        public List<Coupon> ActiveCoupons { get; set; } = new List<Coupon>();
    }
}
