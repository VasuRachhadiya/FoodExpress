using FoodExpress.Data;
using FoodExpress.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Controllers
{
    public class RestaurantsController : Controller
    {
        private readonly FoodExpressDbContext _context;

        public RestaurantsController(FoodExpressDbContext context)
        {
            _context = context;
        }

        // GET: /Restaurants
        public async Task<IActionResult> Index(string? search, string? city)
        {
            var query = _context.Restaurants
                .Where(r => r.IsActive)
                .Include(r => r.Reviews)
                .Include(r => r.FoodItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(r => r.Name.Contains(term) || r.Description.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(r => r.City.ToLower() == city.Trim().ToLower());
            }

            var cities = await _context.Restaurants
                .Where(r => r.IsActive)
                .Select(r => r.City)
                .Distinct()
                .ToListAsync();

            var model = new RestaurantFilterViewModel
            {
                Search = search,
                City = city,
                Restaurants = await query.ToListAsync(),
                Cities = cities
            };

            return View(model);
        }

        // GET: /Restaurants/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.FoodItems)
                    .ThenInclude(f => f.Category)
                .Include(r => r.Reviews.Where(rev => rev.IsApproved))
                    .ThenInclude(rev => rev.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return NotFound();
            }

            // Group food items by category for easy menu navigation
            ViewBag.GroupedMenu = restaurant.FoodItems
                .Where(f => f.IsAvailable)
                .GroupBy(f => f.Category?.Name ?? "General Menu")
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(restaurant);
        }
    }
}
