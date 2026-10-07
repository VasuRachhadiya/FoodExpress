using FoodExpress.Models;

namespace FoodExpress.ViewModels
{
    public class FoodFilterViewModel
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? RestaurantId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? OnlyAvailable { get; set; }
        public string? SortBy { get; set; } // "price_asc", "price_desc", "name", "rating"

        public List<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
        public List<FoodCategory> Categories { get; set; } = new List<FoodCategory>();
        public List<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
    }

    public class RestaurantFilterViewModel
    {
        public string? Search { get; set; }
        public string? City { get; set; }
        public bool? OnlyActive { get; set; }

        public List<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
        public List<string> Cities { get; set; } = new List<string>();
    }
}
