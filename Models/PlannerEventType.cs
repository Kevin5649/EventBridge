namespace EventBridge.Models
{
    public class PlannerEventType
    {
        public string PlannerId { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public virtual EventPlanner Planner { get; set; } = null!;

        public virtual Category Category { get; set; } = null!;
    }
}