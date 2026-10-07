using FoodExpress.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly FoodExpressDbContext _context;

        public CategoriesController(FoodExpressDbContext context)
        {
            _context = context;
        }

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Include(c => c.FoodItems.Where(f => f.IsAvailable))
                .ToListAsync();

            return View(categories);
        }
    }
}
