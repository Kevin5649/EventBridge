using EventBridge.Models;

namespace EventBridge.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public int TotalEnquiries { get; set; }
        public int PendingQuotations { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalPayments { get; set; }
        
        public IEnumerable<Booking> RecentBookings { get; set; } = new List<Booking>();
    }
}
