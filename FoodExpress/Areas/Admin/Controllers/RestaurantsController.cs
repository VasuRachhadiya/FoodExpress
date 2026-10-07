using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

        // GET: /Admin/Restaurants
        public async Task<IActionResult> Index(string? search, string? city, bool? status)
        {
            var query = _context.Restaurants
                .Include(r => r.Owner)
                .Include(r => r.FoodItems)
                .Include(r => r.Orders)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(r => r.Name.Contains(term) || r.Description.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(r => r.City == city);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.IsActive == status.Value);
            }

            ViewBag.Cities = await _context.Restaurants.Select(r => r.City).Distinct().ToListAsync();
            ViewBag.Search = search;
            ViewBag.SelectedCity = city;
            ViewBag.SelectedStatus = status;

            return View(await query.OrderByDescending(r => r.Id).ToListAsync());
        }

        // GET: /Admin/Restaurants/Create
        public async Task<IActionResult> Create()
        {
            await PopulateOwnersDropDownList();
            return View(new Restaurant
            {
                OpeningTime = new TimeSpan(10, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0),
                IsActive = true
            });
        }

        // POST: /Admin/Restaurants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Restaurant restaurant, IFormFile? imageFile)
        {
            ModelState.Remove("Owner");
            ModelState.Remove("Image");
            ModelState.Remove("FoodItems");
            ModelState.Remove("Orders");
            ModelState.Remove("Reviews");

            if (string.IsNullOrWhiteSpace(restaurant.Description))
            {
                restaurant.Description = restaurant.Name;
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    try
                    {
                        restaurant.Image = await _fileService.SaveFileAsync(imageFile, "restaurants");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        await PopulateOwnersDropDownList(restaurant.OwnerId);
                        return View(restaurant);
                    }
                }
                else
                {
                    restaurant.Image = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800&auto=format&fit=crop&q=80";
                }

                restaurant.Phone = Restaurant.FormatIndianPhoneNumber(restaurant.Phone);
                restaurant.CreatedDate = DateTime.UtcNow;
                _context.Restaurants.Add(restaurant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Restaurant '{restaurant.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateOwnersDropDownList(restaurant.OwnerId);
            return View(restaurant);
        }

        // GET: /Admin/Restaurants/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            await PopulateOwnersDropDownList(restaurant.OwnerId);
            return View(restaurant);
        }

        // POST: /Admin/Restaurants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Restaurant model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Owner");
            ModelState.Remove("Image");
            ModelState.Remove("FoodItems");
            ModelState.Remove("Orders");
            ModelState.Remove("Reviews");
            if (ModelState.IsValid)
            {
                var restaurant = await _context.Restaurants.FindAsync(id);
                if (restaurant == null) return NotFound();

                restaurant.Name = model.Name;
                restaurant.Description = model.Description;
                restaurant.Address = model.Address;
                restaurant.City = model.City;
                restaurant.Phone = Restaurant.FormatIndianPhoneNumber(model.Phone);
                restaurant.Email = model.Email;
                restaurant.OpeningTime = model.OpeningTime;
                restaurant.ClosingTime = model.ClosingTime;
                restaurant.IsActive = model.IsActive;
                restaurant.OwnerId = model.OwnerId;

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
                        await PopulateOwnersDropDownList(model.OwnerId);
                        return View(model);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Restaurant '{restaurant.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateOwnersDropDownList(model.OwnerId);
            return View(model);
        }

        // POST: /Admin/Restaurants/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Orders)
                .Include(r => r.FoodItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null) return NotFound();

            if (restaurant.Orders.Any(o => o.OrderStatus != OrderStatus.Delivered && o.OrderStatus != OrderStatus.Cancelled))
            {
                TempData["ErrorMessage"] = $"Cannot delete restaurant '{restaurant.Name}' because it has active pending or preparing orders.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(restaurant.Image) && !restaurant.Image.StartsWith("http"))
            {
                _fileService.DeleteFile(restaurant.Image);
            }

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Restaurant '{restaurant.Name}' was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Restaurants/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            restaurant.IsActive = !restaurant.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Restaurant '{restaurant.Name}' status updated to {(restaurant.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateOwnersDropDownList(object? selectedOwner = null)
        {
            var owners = await _userManager.GetUsersInRoleAsync("RestaurantOwner");
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var combinedUsers = owners.Union(adminUsers).DistinctBy(u => u.Id).ToList();

            ViewBag.Owners = combinedUsers;
            ViewBag.OwnerList = new SelectList(combinedUsers, "Id", "FullName", selectedOwner);
        }
    }
}
