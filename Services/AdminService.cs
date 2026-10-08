using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace EventBridge.Services
{
    public class AdminService : IAdminService
    {
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<EventPlanner> _plannerRepo;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<Review> _reviewRepo;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(
            IRepository<Customer> customerRepo,
            IRepository<EventPlanner> plannerRepo,
            IRepository<Booking> bookingRepo,
            IRepository<Payment> paymentRepo,
            IRepository<Review> reviewRepo,
            UserManager<User> userManager,
            IUnitOfWork unitOfWork)
        {
            _customerRepo = customerRepo;
            _plannerRepo = plannerRepo;
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _reviewRepo = reviewRepo;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        // =========================
        // ACTIVATE USER
        // =========================

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            user.IsActive = true;
            user.Status = "Active";

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }


        // =========================
        // DEACTIVATE USER
        // =========================

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            user.IsActive = false;
            user.Status = "Inactive";

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepo.GetAllAsync("User");
        }

        public async Task<IEnumerable<EventPlanner>> GetAllPlannersAsync()
        {
            return await _plannerRepo.GetAllAsync("User");
        }

        // =========================
        // ADMIN DASHBOARD
        // =========================

        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            var customers = await _customerRepo.GetAllAsync("User");

            var planners = await _plannerRepo.GetAllAsync("User");

            var bookings = await _bookingRepo.GetAllAsync(
                "Quotation.Enquiry.Customer.User,Quotation.Enquiry.EventPlanner.User"
            );

            var payments = await _paymentRepo.FindAsync(
                p => p.Status == "Success"
            );

            var reviews = await _reviewRepo.GetAllAsync();


            // =========================
            // RECENT DATA
            // =========================

            var recentBookings = bookings
                .OrderByDescending(b => b.BookedAt)
                .Take(5)
                .ToList();

            //var recentPlanners = planners
            //    .OrderByDescending(p => p.User?.CreatedAt ?? DateTime.MinValue)
            //    .Take(5)
            //    .ToList();

            var recentPlanners = planners
    .OrderByDescending(p => p.AvgRating)
    .ThenByDescending(p => p.User?.CreatedAt ?? DateTime.MinValue)
    .Take(5)
    .ToList();

            // =========================
            // LAST 6 MONTHS CHART
            // =========================

            var labels = new List<string>();
            var bookingsData = new List<int>();
            var paymentsData = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);

                labels.Add(month.ToString("MMM yyyy"));

                var monthBookings = bookings.Count(
                    b => b.BookedAt.Year == month.Year &&
                         b.BookedAt.Month == month.Month
                );

                var monthPayments = payments
                    .Where(p =>
                        p.PaymentDate.Year == month.Year &&
                        p.PaymentDate.Month == month.Month)
                    .Sum(p => p.Amount);

                bookingsData.Add(monthBookings);
                paymentsData.Add(monthPayments);
            }


            // =========================
            // TOP PLANNERS
            // =========================

            var plannerGroups = bookings
                .Where(b => b.Quotation?.Enquiry?.EventPlanner != null)
                .GroupBy(b =>
                    b.Quotation!.Enquiry!.EventPlanner!.CompanyName
                )
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();


            // =========================
            // REVIEW DISTRIBUTION
            // =========================

            var reviewDist = new List<int>();

            for (int r = 1; r <= 5; r++)
            {
                reviewDist.Add(
                    reviews.Count(rev => rev.Rating == r)
                );
            }


            // =========================
            // RETURN DASHBOARD MODEL
            // =========================

            return new AdminDashboardViewModel
            {
                // Statistics
                TotalCustomers = customers.Count(),
                TotalPlanners = planners.Count(),
                PendingVerifications = planners.Count(p => !p.IsVerified),
                VerifiedPlanners = planners.Count(p => p.IsVerified),
                UnverifiedPlanners = planners.Count(p => !p.IsVerified),

                TotalBookings = bookings.Count(),

                TotalPayments = payments.Sum(p => p.Amount),

                TotalReviews = reviews.Count(),


                // User management
                Customers = customers,
                Planners = planners,


                // Recent data
                RecentPlanners = recentPlanners,
                RecentBookings = recentBookings,


                // Charts
                Labels = labels,
                BookingsData = bookingsData,
                PaymentsData = paymentsData,

                CategoryLabels = plannerGroups
                    .Select(c => c.Name)
                    .ToList(),

                CategoryData = plannerGroups
                    .Select(c => c.Count)
                    .ToList(),

                ReviewDistribution = reviewDist
            };
        }


        // =========================
        // VERIFY PLANNER
        // =========================

        public async Task<bool> VerifyPlannerAsync(
            string plannerId,
            string remarks)
        {
            var planners = await _plannerRepo.FindAsync(
                p => p.PlannerId == plannerId
            );

            var planner = planners.FirstOrDefault();

            if (planner == null)
                return false;

            planner.IsVerified = true;

            _plannerRepo.Update(planner);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // =========================
        // UNVERIFY PLANNER
        // =========================

        public async Task<bool> UnverifyPlannerAsync(
            string plannerId)
        {
            var planners = await _plannerRepo.FindAsync(
                p => p.PlannerId == plannerId
            );

            var planner = planners.FirstOrDefault();

            if (planner == null)
                return false;

            planner.IsVerified = false;

            _plannerRepo.Update(planner);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }


        // =========================
        // REJECT PLANNER
        // =========================

        public async Task<bool> RejectPlannerAsync(
            string plannerId,
            string remarks)
        {
            var planners = await _plannerRepo.FindAsync(
                p => p.PlannerId == plannerId
            );

            var planner = planners.FirstOrDefault();

            if (planner == null)
                return false;

            planner.IsVerified = false;

            _plannerRepo.Update(planner);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}