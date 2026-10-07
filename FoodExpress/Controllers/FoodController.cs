using FoodExpress.Data;
using FoodExpress.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Controllers
{
    public class FoodController : Controller
    {
        private readonly FoodExpressDbContext _context;

        public FoodController(FoodExpressDbContext context)
        {
            _context = context;
        }

        // GET: /Food
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            int? restaurantId,
            decimal? minPrice,
            decimal? maxPrice,
            bool? onlyAvailable,
            string? sortBy)
        {
            var query = _context.FoodItems
                .Include(f => f.Category)
                .Include(f => f.Restaurant)
                .Include(f => f.Reviews)
                .Where(f => f.Restaurant!.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(f => f.Name.Contains(term) || f.Description.Contains(term));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(f => f.CategoryId == categoryId.Value);
            }

            if (restaurantId.HasValue && restaurantId.Value > 0)
            {
                query = query.Where(f => f.RestaurantId == restaurantId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(f => f.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(f => f.Price <= maxPrice.Value);
            }

            if (onlyAvailable.HasValue && onlyAvailable.Value)
            {
                query = query.Where(f => f.IsAvailable);
            }

            // Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(f => f.Price),
                "price_desc" => query.OrderByDescending(f => f.Price),
                "name" => query.OrderBy(f => f.Name),
                _ => query.OrderByDescending(f => f.Id)
            };

            var viewModel = new FoodFilterViewModel
            {
                Search = search,
                CategoryId = categoryId,
                RestaurantId = restaurantId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                OnlyAvailable = onlyAvailable,
                SortBy = sortBy,
                FoodItems = await query.ToListAsync(),
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                Restaurants = await _context.Restaurants.Where(r => r.IsActive).ToListAsync()
            };

            return View(viewModel);
        }

        // GET: /Food/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var foodItem = await _context.FoodItems
                .Include(f => f.Category)
                .Include(f => f.Restaurant)
                .Include(f => f.Reviews.Where(r => r.IsApproved))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (foodItem == null)
            {
                return NotFound();
            }

            ViewBag.RelatedItems = await _context.FoodItems
                .Where(f => f.CategoryId == foodItem.CategoryId && f.Id != foodItem.Id && f.IsAvailable)
                .Take(4)
                .ToListAsync();

            return View(foodItem);
        }
    }
}
