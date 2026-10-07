using FoodExpress.Data;
using FoodExpress.Models;
using FoodExpress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly IFileService _fileService;

        public CategoriesController(FoodExpressDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Categories
                .Include(c => c.FoodItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search.Trim()) || (c.Description != null && c.Description.Contains(search.Trim())));
            }

            ViewBag.Search = search;
            return View(await query.OrderByDescending(c => c.Id).ToListAsync());
        }

        // GET: /Admin/Categories/Create
        public IActionResult Create()
        {
            return View(new FoodCategory { IsActive = true });
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodCategory category, IFormFile? imageFile)
        {
            ModelState.Remove("Image");
            ModelState.Remove("FoodItems");

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    try
                    {
                        category.Image = await _fileService.SaveFileAsync(imageFile, "categories");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        return View(category);
                    }
                }
                else
                {
                    category.Image = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=500&auto=format&fit=crop&q=80";
                }

                category.CreatedDate = DateTime.UtcNow;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Category '{category.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: /Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodCategory model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Image");
            ModelState.Remove("FoodItems");

            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return NotFound();

                category.Name = model.Name;
                category.Description = model.Description;
                category.IsActive = model.IsActive;

                if (imageFile != null)
                {
                    try
                    {
                        var newImg = await _fileService.SaveFileAsync(imageFile, "categories");
                        if (!string.IsNullOrEmpty(category.Image) && !category.Image.StartsWith("http"))
                        {
                            _fileService.DeleteFile(category.Image);
                        }
                        category.Image = newImg;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        return View(model);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category '{category.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.FoodItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            if (category.FoodItems.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete category '{category.Name}' because {category.FoodItems.Count} food item(s) are assigned to it. Please reassign or delete the food items first.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(category.Image) && !category.Image.StartsWith("http"))
            {
                _fileService.DeleteFile(category.Image);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' deleted.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Categories/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' status updated to {(category.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
