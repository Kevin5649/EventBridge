using EventBridge.Models;

namespace EventBridge.ViewModels
{
    public class PlannerDashboardViewModel
    {
        public int PendingEnquiries { get; set; }
        public int PendingQuotations { get; set; }
        public int UpcomingEvents { get; set; }
        public int CompletedEvents { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal Earnings { get; set; }

        public IEnumerable<Booking> RecentBookings { get; set; } = new List<Booking>();
    }
}
