using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly UserManager<User> _userManager;
        private readonly IBookingService _bookingService;

        public ReviewController(IReviewService reviewService, UserManager<User> userManager, IBookingService bookingService)
        {
            _reviewService = reviewService;
            _userManager = userManager;
            _bookingService = bookingService;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Create(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!await _reviewService.CanReviewAsync(bookingId, user.Id))
            {
                return BadRequest("You are not eligible to review this booking.");
            }

            var model = new ReviewCreateViewModel
            {
                BookingId = bookingId
            };

            return View(model);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var success = await _reviewService.CreateReviewAsync(user.Id, model);
            if (success)
            {
                TempData["SuccessMessage"] = "Thank you for your review!";
                return RedirectToAction("Details", "Booking", new { id = model.BookingId });
            }

            ModelState.AddModelError("", "Failed to submit review.");
            return View(model);
        }
    }
}
