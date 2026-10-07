using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "RestaurantOwner")]
    public class FoodItemsController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;

        public FoodItemsController(
            FoodExpressDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        // GET: /Owner/FoodItems
        public async Task<IActionResult> Index(int? restaurantId, int? categoryId, string? search)
        {
            var userId = _userManager.GetUserId(User);
            var ownedRestaurantIds = await _context.Restaurants
                .Where(r => r.OwnerId == userId)
                .Select(r => r.Id)
                .ToListAsync();

            var query = _context.FoodItems
                .Where(f => ownedRestaurantIds.Contains(f.RestaurantId))
                .Include(f => f.Category)
                .Include(f => f.Restaurant)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f => f.Name.Contains(search.Trim()) || f.Description.Contains(search.Trim()));
            }

            if (restaurantId.HasValue && restaurantId.Value > 0)
            {
                query = query.Where(f => f.RestaurantId == restaurantId.Value);
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(f => f.CategoryId == categoryId.Value);
            }

            ViewBag.OwnedRestaurants = new SelectList(await _context.Restaurants.Where(r => r.OwnerId == userId).ToListAsync(), "Id", "Name", restaurantId);
            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", categoryId);
            ViewBag.Search = search;

            return View(await query.OrderByDescending(f => f.Id).ToListAsync());
        }

        // GET: /Owner/FoodItems/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            await PopulateDropDowns(userId);
            return View(new FoodItem { IsAvailable = true, Price = 9.99m });
        }

        // POST: /Owner/FoodItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItem foodItem, IFormFile? imageFile)
        {
            var userId = _userManager.GetUserId(User);

            // Verify the restaurant belongs to the current user
            var ownsRestaurant = await _context.Restaurants.AnyAsync(r => r.Id == foodItem.RestaurantId && r.OwnerId == userId);
            if (!ownsRestaurant)
            {
                ModelState.AddModelError("RestaurantId", "You can only add dishes to your own restaurants.");
            }

            ModelState.Remove("Category");
            ModelState.Remove("Restaurant");
            ModelState.Remove("Image");
            ModelState.Remove("CartItems");
            ModelState.Remove("OrderItems");
            ModelState.Remove("Reviews");

            if (string.IsNullOrWhiteSpace(foodItem.Description))
            {
                foodItem.Description = foodItem.Name;
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    try
                    {
                        foodItem.Image = await _fileService.SaveFileAsync(imageFile, "foods");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        await PopulateDropDowns(userId, foodItem.CategoryId, foodItem.RestaurantId);
                        return View(foodItem);
                    }
                }
                else
                {
                    foodItem.Image = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=600&auto=format&fit=crop&q=80";
                }

                foodItem.CreatedDate = DateTime.UtcNow;
                _context.FoodItems.Add(foodItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Dish '{foodItem.Name}' added to menu!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDowns(userId, foodItem.CategoryId, foodItem.RestaurantId);
            return View(foodItem);
        }

        // GET: /Owner/FoodItems/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.FoodItems
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == id && f.Restaurant!.OwnerId == userId);

            if (item == null) return NotFound();

            await PopulateDropDowns(userId, item.CategoryId, item.RestaurantId);
            return View(item);
        }

        // POST: /Owner/FoodItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodItem model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            var userId = _userManager.GetUserId(User);
            var item = await _context.FoodItems
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == id && f.Restaurant!.OwnerId == userId);

            if (item == null) return NotFound();

            var ownsRestaurant = await _context.Restaurants.AnyAsync(r => r.Id == model.RestaurantId && r.OwnerId == userId);
            if (!ownsRestaurant)
            {
                ModelState.AddModelError("RestaurantId", "You can only assign dishes to your own restaurants.");
            }

            ModelState.Remove("Category");
            ModelState.Remove("Restaurant");
            ModelState.Remove("Image");
            ModelState.Remove("CartItems");
            ModelState.Remove("OrderItems");
            ModelState.Remove("Reviews");

            if (ModelState.IsValid)
            {
                item.Name = model.Name;
                item.Description = model.Description;
                item.Price = model.Price;
                item.CategoryId = model.CategoryId;
                item.RestaurantId = model.RestaurantId;
                item.IsAvailable = model.IsAvailable;

                if (imageFile != null)
                {
                    try
                    {
                        var newImg = await _fileService.SaveFileAsync(imageFile, "foods");
                        if (!string.IsNullOrEmpty(item.Image) && !item.Image.StartsWith("http"))
                        {
                            _fileService.DeleteFile(item.Image);
                        }
                        item.Image = newImg;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        await PopulateDropDowns(userId, model.CategoryId, model.RestaurantId);
                        return View(model);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Dish '{item.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDowns(userId, model.CategoryId, model.RestaurantId);
            return View(model);
        }

        // POST: /Owner/FoodItems/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.FoodItems
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == id && f.Restaurant!.OwnerId == userId);

            if (item == null) return NotFound();

            if (!string.IsNullOrEmpty(item.Image) && !item.Image.StartsWith("http"))
            {
                _fileService.DeleteFile(item.Image);
            }

            _context.FoodItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Dish '{item.Name}' removed from menu.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Owner/FoodItems/ToggleAvailability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.FoodItems
                .Include(f => f.Restaurant)
                .FirstOrDefaultAsync(f => f.Id == id && f.Restaurant!.OwnerId == userId);

            if (item == null) return NotFound();

            item.IsAvailable = !item.IsAvailable;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{item.Name}' availability updated to {(item.IsAvailable ? "Available" : "Sold Out")}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDowns(string? userId, object? selectedCategory = null, object? selectedRestaurant = null)
        {
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            var restaurants = await _context.Restaurants.Where(r => r.OwnerId == userId).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Restaurants = restaurants;
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedCategory);
            ViewBag.RestaurantId = new SelectList(restaurants, "Id", "Name", selectedRestaurant);
        }
    }
}
