using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "RestaurantOwner")]
    public class RestaurantsController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;

        public RestaurantsController(
            FoodExpressDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        // GET: /Owner/Restaurants
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var restaurants = await _context.Restaurants
                .Where(r => r.OwnerId == userId)
                .Include(r => r.FoodItems)
                .Include(r => r.Orders)
                .ToListAsync();

            return View(restaurants);
        }

        // GET: /Owner/Restaurants/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == userId);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // POST: /Owner/Restaurants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Restaurant model, IFormFile? imageFile)
        {
            var userId = _userManager.GetUserId(User);
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == userId);
            if (restaurant == null)
            {
                return NotFound();
            }

            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                restaurant.Name = model.Name;
                restaurant.Description = model.Description;
                restaurant.Address = model.Address;
                restaurant.City = model.City;
                restaurant.Phone = Restaurant.FormatIndianPhoneNumber(model.Phone);
                restaurant.Email = model.Email;
                restaurant.OpeningTime = model.OpeningTime;
                restaurant.ClosingTime = model.ClosingTime;
                restaurant.IsActive = model.IsActive;

                if (imageFile != null)
                {
                    try
                    {
                        var newImg = await _fileService.SaveFileAsync(imageFile, "restaurants");
                        if (!string.IsNullOrEmpty(restaurant.Image) && !restaurant.Image.StartsWith("http"))
                        {
                            _fileService.DeleteFile(restaurant.Image);
                        }
                        restaurant.Image = newImg;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        return View(model);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Restaurant '{restaurant.Name}' details updated!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /Owner/Restaurants/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = _userManager.GetUserId(User);
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == userId);
            if (restaurant == null) return NotFound();

            restaurant.IsActive = !restaurant.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Restaurant is now {(restaurant.IsActive ? "Open / Active" : "Closed / Inactive")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
