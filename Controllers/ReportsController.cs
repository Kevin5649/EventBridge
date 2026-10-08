using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EventBridge.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<EventPlanner> _plannerRepo;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<Review> _reviewRepo;
        private readonly IRepository<Category> _categoryRepo;

        public ReportsController(
            IRepository<Customer> customerRepo,
            IRepository<EventPlanner> plannerRepo,
            IRepository<Booking> bookingRepo,
            IRepository<Payment> paymentRepo,
            IRepository<Review> reviewRepo,
            IRepository<Category> categoryRepo)
        {
            _customerRepo = customerRepo;
            _plannerRepo = plannerRepo;
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _reviewRepo = reviewRepo;
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Customers(string searchString, string sortOrder)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.CurrentFilter = searchString;

            var customers = await _customerRepo.GetAllAsync("User");

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => 
                    c.User.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    c.User.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            customers = sortOrder switch
            {
                "name_desc" => customers.OrderByDescending(c => c.User?.FullName),
                "Date" => customers.OrderBy(c => c.User?.CreatedAt),
                "date_desc" => customers.OrderByDescending(c => c.User?.CreatedAt),
                _ => customers.OrderBy(c => c.User?.FullName)
            };

            return View(customers);
        }

        public async Task<IActionResult> Planners(string searchString, string sortOrder, string statusFilter)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.ExpSortParm = sortOrder == "Exp" ? "exp_desc" : "Exp";
            ViewBag.CurrentFilter = searchString;
            ViewBag.StatusFilter = statusFilter;

            var planners = await _plannerRepo.GetAllAsync("User");

            if (!string.IsNullOrEmpty(searchString))
            {
                planners = planners.Where(p => 
                    p.CompanyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.User.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isVerified = statusFilter == "verified";
                planners = planners.Where(p => p.IsVerified == isVerified);
            }

            planners = sortOrder switch
            {
                "name_desc" => planners.OrderByDescending(p => p.CompanyName),
                "Exp" => planners.OrderBy(p => p.Experience),
                "exp_desc" => planners.OrderByDescending(p => p.Experience),
                _ => planners.OrderBy(p => p.CompanyName)
            };

            return View(planners);
        }

        public async Task<IActionResult> Bookings(string searchString, string statusFilter)
        {
            ViewBag.CurrentFilter = searchString;
            ViewBag.StatusFilter = statusFilter;

            var bookings = await _bookingRepo.GetAllAsync("Quotation.Enquiry.Customer.User,Quotation.Enquiry.EventPlanner.User");

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b => 
                    b.Id.ToString().Contains(searchString) ||
                    b.Quotation.Enquiry.EventPlanner.CompanyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    b.Quotation.Enquiry.Customer.User.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bookings = bookings.Where(b => b.Status == statusFilter);
            }

            return View(bookings.OrderByDescending(b => b.BookedAt));
        }

        public async Task<IActionResult> Payments(string searchString)
        {
            var payments = await _paymentRepo.GetAllAsync(
                "Booking.Quotation.Enquiry.Customer.User,Booking.Quotation.Enquiry.EventPlanner.User"
            );

            if (!string.IsNullOrEmpty(searchString))
            {
                payments = payments.Where(p => 
                    p.TransactionId.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.BookingId.ToString().Contains(searchString));
            }

            return View(payments.OrderByDescending(p => p.PaymentDate));
        }

        public async Task<IActionResult> Reviews(int? ratingFilter)
        {
            ViewBag.RatingFilter = ratingFilter;
            var reviews = await _reviewRepo.GetAllAsync("Customer.User,EventPlanner.User,Booking");

            if (ratingFilter.HasValue)
            {
                reviews = reviews.Where(r => r.Rating == ratingFilter.Value);
            }

            return View(reviews.OrderByDescending(r => r.CreatedAt));
        }

        public async Task<IActionResult> Categories()
        {
            var categories =
                await _categoryRepo.GetAllAsync("PlannerEventTypes");

            var report = categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryReportViewModel
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    PlannerCount = c.PlannerEventTypes.Count
                })
                .ToList();

            return View(report);
        }
    }
}
