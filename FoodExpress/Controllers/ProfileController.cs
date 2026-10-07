using FoodExpress.Models;
using FoodExpress.Services;
using FoodExpress.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FoodExpress.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IFileService _fileService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileService = fileService;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                ExistingProfileImage = user.ProfileImage
            };

            return View(vm);
        }

        // POST: /Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                model.ExistingProfileImage = user.ProfileImage;
                model.Email = user.Email ?? "";
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            user.Address = model.Address?.Trim();
            user.City = model.City?.Trim();
            user.State = model.State?.Trim();
            user.PostalCode = model.PostalCode?.Trim();

            if (model.ProfileImageFile != null)
            {
                try
                {
                    var newImage = await _fileService.SaveFileAsync(model.ProfileImageFile, "profiles");
                    if (!string.IsNullOrEmpty(user.ProfileImage))
                    {
                        _fileService.DeleteFile(user.ProfileImage);
                    }
                    user.ProfileImage = newImage;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ProfileImageFile", ex.Message);
                    model.ExistingProfileImage = user.ProfileImage;
                    model.Email = user.Email ?? "";
                    return View(model);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile details updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.ExistingProfileImage = user.ProfileImage;
            model.Email = user.Email ?? "";
            return View(model);
        }

        // GET: /Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your password has been changed successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
