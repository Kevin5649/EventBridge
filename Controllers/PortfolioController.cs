using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    [Authorize(Roles = "EventPlanner")]
    public class PortfolioController : Controller
    {
        private readonly IPortfolioService _portfolioService;
        private readonly UserManager<User> _userManager;
        private readonly IFileService _fileService;

        public PortfolioController(IPortfolioService portfolioService, UserManager<User> userManager, IFileService fileService)
        {
            _portfolioService = portfolioService;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var images = await _portfolioService.GetPortfolioAsync(user.Id);
            return View(images);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(PortfolioUploadViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int currentCount = await _portfolioService.GetPortfolioCountAsync(user.Id);
            if (currentCount >= 10)
            {
                ModelState.AddModelError("", "You can only upload a maximum of 10 images.");
                return View(model);
            }

            try
            {
                var imagePath = await _fileService.SaveFileAsync(model.Image, "portfolio");
                await _portfolioService.AddImageAsync(user.Id, imagePath, model.Caption ?? "");
                TempData["SuccessMessage"] = "Image uploaded successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var success = await _portfolioService.DeleteImageAsync(id, user.Id);
            if (success)
            {
                TempData["SuccessMessage"] = "Image deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete image.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
