using FoodExpress.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(string? search, string? roleFilter)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u => u.Email!.Contains(term) || u.FullName.Contains(term) || (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
            }

            var users = await query.OrderByDescending(u => u.CreatedDate).ToListAsync();
            var userListWithRoles = new List<(ApplicationUser User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (string.IsNullOrEmpty(roleFilter) || roles.Contains(roleFilter))
                {
                    userListWithRoles.Add((user, roles));
                }
            }

            ViewBag.Search = search;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            return View(userListWithRoles);
        }

        // GET: /Admin/Users/Details/id
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.Users
                .Include(u => u.OwnedRestaurants)
                .Include(u => u.Orders)
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            return View(user);
        }

        // POST: /Admin/Users/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own administrative account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"User '{user.FullName}' has been {(user.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id, string newRole)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot alter your own administrative role.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                TempData["ErrorMessage"] = "Selected role does not exist.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["SuccessMessage"] = $"Role for {user.FullName} changed to {newRole}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
