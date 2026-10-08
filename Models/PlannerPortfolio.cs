using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBridge.Models
{
    public class PlannerPortfolio
    {
        [Key]
        public int PortfolioId { get; set; }

        [Required]
        [ForeignKey("EventPlanner")]
        public string PlannerId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Caption { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public virtual EventPlanner EventPlanner { get; set; } = null!;
    }
}
