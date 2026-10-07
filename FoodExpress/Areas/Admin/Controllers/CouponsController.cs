using FoodExpress.Data;
using FoodExpress.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponsController : Controller
    {
        private readonly FoodExpressDbContext _context;

        public CouponsController(FoodExpressDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Coupons
        public async Task<IActionResult> Index()
        {
            return View(await _context.Coupons.OrderByDescending(c => c.Id).ToListAsync());
        }

        // GET: /Admin/Coupons/Create
        public IActionResult Create()
        {
            return View(new Coupon
            {
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(1),
                IsActive = true,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 15m
            });
        }

        // POST: /Admin/Coupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                coupon.Code = coupon.Code.Trim().ToUpperInvariant();

                if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code))
                {
                    ModelState.AddModelError("Code", "A coupon with this code already exists.");
                    return View(coupon);
                }

                if (coupon.EndDate < coupon.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(coupon);
                }

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        // GET: /Admin/Coupons/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            return View(coupon);
        }

        // POST: /Admin/Coupons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null) return NotFound();

                model.Code = model.Code.Trim().ToUpperInvariant();

                if (await _context.Coupons.AnyAsync(c => c.Code == model.Code && c.Id != id))
                {
                    ModelState.AddModelError("Code", "Another coupon with this code already exists.");
                    return View(model);
                }

                coupon.Code = model.Code;
                coupon.Description = model.Description;
                coupon.DiscountType = model.DiscountType;
                coupon.DiscountValue = model.DiscountValue;
                coupon.MinimumOrderAmount = model.MinimumOrderAmount;
                coupon.StartDate = model.StartDate;
                coupon.EndDate = model.EndDate;
                coupon.IsActive = model.IsActive;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /Admin/Coupons/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' removed.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Coupons/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' status set to {(coupon.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
