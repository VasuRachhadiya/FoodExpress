using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FoodExpress.Controllers
{
    public class HomeController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(FoodExpressDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                Categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .Include(c => c.FoodItems)
                    .OrderBy(c => c.Id)
                    .Take(6)
                    .ToListAsync(),

                FeaturedRestaurants = await _context.Restaurants
                    .Where(r => r.IsActive)
                    .Include(r => r.Reviews)
                    .Include(r => r.FoodItems)
                    .OrderByDescending(r => r.Id)
                    .Take(6)
                    .ToListAsync(),

                PopularFoodItems = await _context.FoodItems
                    .Where(f => f.IsAvailable && f.Restaurant!.IsActive)
                    .Include(f => f.Restaurant)
                    .Include(f => f.Category)
                    .OrderByDescending(f => f.Id)
                    .Take(8)
                    .ToListAsync(),

                ActiveCoupons = await _context.Coupons
                    .Where(c => c.IsActive && c.EndDate >= DateTime.UtcNow.Date)
                    .OrderBy(c => c.EndDate)
                    .Take(2)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return View();
            }

            TempData["SuccessMessage"] = "Thank you for contacting FoodExpress! Our team will get back to you within 24 hours.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
