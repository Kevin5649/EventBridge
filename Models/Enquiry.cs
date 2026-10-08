using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBridge.Models
{
    public class Enquiry
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("EventPlanner")]
        public string PlannerId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }

        [Required]
        public int GuestCount { get; set; }

        [Required]
        [StringLength(500)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string RequiredServices { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public virtual Customer Customer { get; set; } = null!;
        public virtual EventPlanner EventPlanner { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual Quotation? Quotation { get; set; }
    }
}