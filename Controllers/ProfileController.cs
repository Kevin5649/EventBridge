using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly UserManager<User> _userManager;

        public ProfileController(IProfileService profileService, UserManager<User> userManager)
        {
            _profileService = profileService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = await _profileService.GetProfileAsync(user.Id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // We must remove some validations depending on role
            if (user.Role != "Customer") ModelState.Remove("Address");
            if (user.Role != "EventPlanner")
            {
                ModelState.Remove("CompanyName");
                ModelState.Remove("Description");
                ModelState.Remove("Experience");
            }

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? string.Empty;
                model.Role = user.Role;
                model.ProfilePicturePath = user.ProfilePicturePath;

                ModelState.Remove("Email");

                return View("Index", model);
            }

            var result = await _profileService.UpdateProfileAsync(user.Id, model);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Email = user.Email ?? string.Empty;
            model.Role = user.Role;
            model.ProfilePicturePath = user.ProfilePicturePath;

            ModelState.Remove("Email");

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _profileService.ChangePasswordAsync(user.Id, model);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
