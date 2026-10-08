using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBridge.Models
{
    public class EventPlanner
    {
        [Key]
        [ForeignKey("User")]
        public string PlannerId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public int Experience { get; set; }

        public decimal AvgRating { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;

        public bool IsVerified { get; set; } = false;

        public DateTime? VerificationDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<PlannerPortfolio> Portfolios { get; set; } = new List<PlannerPortfolio>();
        public virtual ICollection<PlannerEventType> PlannerEventTypes { get; set; } = new List<PlannerEventType>();
    }
}
