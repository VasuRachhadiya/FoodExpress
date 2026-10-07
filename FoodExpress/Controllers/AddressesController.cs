using FoodExpress.Data;
using FoodExpress.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Controllers
{
    [Authorize]
    public class AddressesController : Controller
    {
        private readonly FoodExpressDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressesController(FoodExpressDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Addresses
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.Id)
                .ToListAsync();

            return View(addresses);
        }

        // GET: /Addresses/Create
        public IActionResult Create()
        {
            return View(new Address { Title = "Home" });
        }

        // POST: /Addresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Address address)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                return View(address);
            }

            address.UserId = user.Id;

            // If marked as default or is the first address, unset others
            if (address.IsDefault || !await _context.Addresses.AnyAsync(a => a.UserId == user.Id))
            {
                address.IsDefault = true;
                var existingDefaults = await _context.Addresses.Where(a => a.UserId == user.Id && a.IsDefault).ToListAsync();
                foreach (var d in existingDefaults) d.IsDefault = false;
            }

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Delivery address added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Addresses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            if (address == null) return NotFound();

            return View(address);
        }

        // POST: /Addresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Address model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            if (address == null) return NotFound();

            address.Title = model.Title;
            address.AddressLine = model.AddressLine;
            address.City = model.City;
            address.State = model.State;
            address.PostalCode = model.PostalCode;
            address.PhoneNumber = model.PhoneNumber;

            if (model.IsDefault && !address.IsDefault)
            {
                var existingDefaults = await _context.Addresses.Where(a => a.UserId == user.Id && a.IsDefault).ToListAsync();
                foreach (var d in existingDefaults) d.IsDefault = false;
                address.IsDefault = true;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Address updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Addresses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            if (address == null) return NotFound();

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Address removed.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Addresses/SetDefault/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            if (address == null) return NotFound();

            var existing = await _context.Addresses.Where(a => a.UserId == user.Id).ToListAsync();
            foreach (var a in existing) a.IsDefault = (a.Id == id);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"'{address.Title}' is now your default address.";
            return RedirectToAction(nameof(Index));
        }
    }
}
