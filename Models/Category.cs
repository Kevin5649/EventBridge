using System.ComponentModel.DataAnnotations;

namespace EventBridge.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<PlannerEventType> PlannerEventTypes { get; set; } = new List<PlannerEventType>();
    }
}
