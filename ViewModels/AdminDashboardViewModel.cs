using EventBridge.Models;

namespace EventBridge.ViewModels
{
    public class AdminDashboardViewModel
    {
        // =========================
        // DASHBOARD STATISTICS
        // =========================

        public int TotalCustomers { get; set; }

        public int TotalPlanners { get; set; }

        public int PendingVerifications { get; set; }

        public int TotalBookings { get; set; }

        public decimal TotalPayments { get; set; }

        public int TotalReviews { get; set; }


        // =========================
        // USER MANAGEMENT
        // =========================

        public IEnumerable<Customer> Customers { get; set; }
            = new List<Customer>();

        public IEnumerable<EventPlanner> Planners { get; set; }
            = new List<EventPlanner>();


        // =========================
        // RECENT DATA
        // =========================

        public IEnumerable<EventPlanner> RecentPlanners { get; set; }
            = new List<EventPlanner>();

        public IEnumerable<Booking> RecentBookings { get; set; }
            = new List<Booking>();


        // =========================
        // CHART DATA
        // =========================

        public List<string> Labels { get; set; }
            = new List<string>();

        public List<int> BookingsData { get; set; }
            = new List<int>();

        public List<decimal> PaymentsData { get; set; }
            = new List<decimal>();

        public List<string> CategoryLabels { get; set; }
            = new List<string>();

        public List<int> CategoryData { get; set; }
            = new List<int>();

        public int VerifiedPlanners { get; set; }

        public int UnverifiedPlanners { get; set; }

        public List<int> ReviewDistribution { get; set; }
            = new List<int>();
    }
}