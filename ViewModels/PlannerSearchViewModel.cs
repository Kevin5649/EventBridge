using EventBridge.Models;

namespace EventBridge.ViewModels
{
    public class PlannerSearchViewModel
    {
        public string? CompanyName { get; set; }

        public int? CategoryId { get; set; }

        public int? MinExperience { get; set; }

        public int? MinRating { get; set; }

        public bool OnlyVerified { get; set; }

        public IEnumerable<EventPlanner> Planners { get; set; }
            = new List<EventPlanner>();
    }
}