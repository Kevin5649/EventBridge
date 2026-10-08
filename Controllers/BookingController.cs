using EventBridge.Interfaces;
using EventBridge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly UserManager<User> _userManager;

        public BookingController(IBookingService bookingService, UserManager<User> userManager)
        {
            _bookingService = bookingService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            IEnumerable<Booking> bookings;
            if (user.Role == "Customer")
            {
                bookings = await _bookingService.GetBookingsByCustomerAsync(user.Id);
            }
            else if (user.Role == "EventPlanner")
            {
                bookings = await _bookingService.GetBookingsByPlannerAsync(user.Id);
            }
            else
            {
                return Forbid();
            }

            return View(bookings.OrderByDescending(b => b.Id));
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            if (user.Role == "Customer" && booking.Quotation?.Enquiry?.CustomerId != user.Id) return Forbid();
            if (user.Role == "EventPlanner" && booking.Quotation?.Enquiry?.PlannerId != user.Id) return Forbid();

            return View(booking);
        }

        [Authorize(Roles = "EventPlanner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var success = await _bookingService.UpdateBookingStatusAsync(id, status, user.Id);
            if (success)
            {
                TempData["SuccessMessage"] = $"Booking marked as {status}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update booking status.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
