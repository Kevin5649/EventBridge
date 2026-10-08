using EventBridge.Models;
using EventBridge.ViewModels;
using EventBridge.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IAdminService _adminService;
        private readonly IEnquiryService _enquiryService;
        private readonly IBookingService _bookingService;
        private readonly IPortfolioService _portfolioService;
        private readonly IRepository<Quotation> _quotationRepo;
        private readonly IRepository<Review> _reviewRepo;
        private readonly IRepository<Payment> _paymentRepo;

        public DashboardController(
            UserManager<User> userManager,
            IAdminService adminService,
            IEnquiryService enquiryService,
            IBookingService bookingService,
            IPortfolioService portfolioService,
            IRepository<Quotation> quotationRepo,
            IRepository<Review> reviewRepo,
            IRepository<Payment> paymentRepo)
        {
            _userManager = userManager;
            _adminService = adminService;
            _enquiryService = enquiryService;
            _bookingService = bookingService;
            _portfolioService = portfolioService;
            _quotationRepo = quotationRepo;
            _reviewRepo = reviewRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.Role == "Admin")
            {
                var stats = await _adminService.GetDashboardStatsAsync();
                return View("AdminIndex", stats);
            }
            else if (user.Role == "Customer")
            {
                var enquiries = await _enquiryService.GetEnquiriesByCustomerAsync(user.Id);
                var bookings = await _bookingService.GetBookingsByCustomerAsync(user.Id);
                var quotations = await _quotationRepo.FindAsync(q => q.Enquiry.CustomerId == user.Id && q.Status == "Pending");
                var payments = await _paymentRepo.FindAsync(p => p.Booking.Quotation.Enquiry.CustomerId == user.Id && p.Status == "Success");

                var model = new CustomerDashboardViewModel
                {
                    TotalEnquiries = enquiries.Count(),
                    PendingQuotations = quotations.Count(),
                    ActiveBookings = bookings.Count(b => b.Status == "Upcoming" || b.Status == "Confirmed"),
                    CompletedBookings = bookings.Count(b => b.Status == "Completed"),
                    TotalPayments = payments.Sum(p => p.Amount),
                    RecentBookings = bookings.OrderByDescending(b => b.BookedAt).Take(5)
                };
                
                return View("CustomerIndex", model);
            }
            else if (user.Role == "EventPlanner")
            {
                var enquiries = await _enquiryService.GetEnquiriesByPlannerAsync(user.Id);
                var bookings = await _bookingService.GetBookingsByPlannerAsync(user.Id);
                var quotations = await _quotationRepo.FindAsync(q => q.Enquiry.PlannerId == user.Id && q.Status == "Pending");
                var reviews = await _reviewRepo.FindAsync(r => r.Booking.Quotation.Enquiry.PlannerId == user.Id);
                var payments = await _paymentRepo.FindAsync(p => p.Booking.Quotation.Enquiry.PlannerId == user.Id && p.Status == "Success");

                var model = new PlannerDashboardViewModel
                {
                    PendingEnquiries = enquiries.Count(e => e.Status == "Pending"),
                    PendingQuotations = quotations.Count(),
                    UpcomingEvents = bookings.Count(b => b.Status == "Upcoming" || b.Status == "Confirmed"),
                    CompletedEvents = bookings.Count(b => b.Status == "Completed"),
                    AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0,
                    TotalReviews = reviews.Count(),
                    Earnings = payments.Sum(p => p.Amount),
                    RecentBookings = bookings.OrderByDescending(b => b.BookedAt).Take(5)
                };
                
                return View("PlannerIndex", model);
            }
            else
            {
                return View("CustomerIndex");
            }
        }
    }
}
