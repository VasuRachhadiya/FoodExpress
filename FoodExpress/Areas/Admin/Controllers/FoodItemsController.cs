using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FoodItemsController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly IFileService _fileService;

        public FoodItemsController(FoodExpressDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /Admin/FoodItems
        public async Task<IActionResult> Index(string? search, int? categoryId, int? restaurantId, bool? isAvailable)
        {
            var query = _context.FoodItems
                .Include(f => f.Category)
                .Include(f => f.Restaurant)
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

            if (isAvailable.HasValue)
            {
                query = query.Where(f => f.IsAvailable == isAvailable.Value);
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.Restaurants = new SelectList(await _context.Restaurants.ToListAsync(), "Id", "Name", restaurantId);
            ViewBag.Search = search;
            ViewBag.IsAvailable = isAvailable;

            return View(await query.OrderByDescending(f => f.Id).ToListAsync());
        }

        // GET: /Admin/FoodItems/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View(new FoodItem { IsAvailable = true, Price = 9.99m });
        }

        // POST: /Admin/FoodItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItem foodItem, IFormFile? imageFile)
        {
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
                        await PopulateDropDowns(foodItem.CategoryId, foodItem.RestaurantId);
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

                TempData["SuccessMessage"] = $"Dish '{foodItem.Name}' added successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDowns(foodItem.CategoryId, foodItem.RestaurantId);
            return View(foodItem);
        }

        // GET: /Admin/FoodItems/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.FoodItems.FindAsync(id);
            if (item == null) return NotFound();

            await PopulateDropDowns(item.CategoryId, item.RestaurantId);
            return View(item);
        }

        // POST: /Admin/FoodItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodItem model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Category");
            ModelState.Remove("Restaurant");
            ModelState.Remove("Image");
            ModelState.Remove("CartItems");
            ModelState.Remove("OrderItems");
            ModelState.Remove("Reviews");

            if (ModelState.IsValid)
            {
                var item = await _context.FoodItems.FindAsync(id);
                if (item == null) return NotFound();

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
                        await PopulateDropDowns(model.CategoryId, model.RestaurantId);
                        return View(model);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Dish '{item.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDowns(model.CategoryId, model.RestaurantId);
            return View(model);
        }

        // POST: /Admin/FoodItems/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.FoodItems.FindAsync(id);
            if (item == null) return NotFound();

            if (!string.IsNullOrEmpty(item.Image) && !item.Image.StartsWith("http"))
            {
                _fileService.DeleteFile(item.Image);
            }

            _context.FoodItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Dish '{item.Name}' deleted.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/FoodItems/ToggleAvailability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var item = await _context.FoodItems.FindAsync(id);
            if (item == null) return NotFound();

            item.IsAvailable = !item.IsAvailable;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{item.Name}' is now {(item.IsAvailable ? "Available" : "Unavailable")}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDowns(object? selectedCategory = null, object? selectedRestaurant = null)
        {
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            var restaurants = await _context.Restaurants.Where(r => r.IsActive).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Restaurants = restaurants;
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedCategory);
            ViewBag.RestaurantId = new SelectList(restaurants, "Id", "Name", selectedRestaurant);
        }
    }
}
