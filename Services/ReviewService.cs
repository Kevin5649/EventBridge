using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;

namespace EventBridge.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepository<Review> _reviewRepo;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<EventPlanner> _plannerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IRepository<Review> reviewRepo, IRepository<Booking> bookingRepo, IRepository<EventPlanner> plannerRepo, IUnitOfWork unitOfWork)
        {
            _reviewRepo = reviewRepo;
            _bookingRepo = bookingRepo;
            _plannerRepo = plannerRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CanReviewAsync(int bookingId, string customerId)
        {
            var bookings = await _bookingRepo.FindAsync(b => b.Id == bookingId && b.Quotation.Enquiry.CustomerId == customerId && b.Status == "Completed", "Quotation.Enquiry");
            var booking = bookings.FirstOrDefault();
            if (booking == null) return false;

            // Check if review already exists
            var existingReviews = await _reviewRepo.FindAsync(r => r.BookingId == bookingId);
            if (existingReviews.Any()) return false;

            return true;
        }

        public async Task<bool> CreateReviewAsync(string customerId, ReviewCreateViewModel model)
        {
            if (!await CanReviewAsync(model.BookingId, customerId)) return false;

            var bookings = await _bookingRepo.FindAsync(b => b.Id == model.BookingId, "Quotation.Enquiry");
            var booking = bookings.FirstOrDefault();
            if (booking == null) return false;

            var review = new Review
            {
                BookingId = model.BookingId,
                CustomerId = customerId,
                PlannerId = booking.Quotation!.Enquiry!.PlannerId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.Now
            };

            await _reviewRepo.AddAsync(review);

            // Update Planner Average Rating and Review Count
            var planners = await _plannerRepo.FindAsync(p => p.PlannerId == review.PlannerId);
            var planner = planners.FirstOrDefault();
            if (planner != null)
            {
                // We need to calculate new average. 
                // Since this review isn't committed yet, we add it manually to calculation
                var existingReviews = await _reviewRepo.FindAsync(r => r.PlannerId == review.PlannerId);
                var totalRating = existingReviews.Sum(r => r.Rating) + review.Rating;
                var totalCount = existingReviews.Count() + 1;

                planner.ReviewCount = totalCount;
                planner.AvgRating = (decimal)totalRating / totalCount;

                _plannerRepo.Update(planner);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Review>> GetReviewsByPlannerAsync(string plannerId)
        {
            return await _reviewRepo.FindAsync(r => r.PlannerId == plannerId, includeProperties: "Customer.User");
        }
    }
}
