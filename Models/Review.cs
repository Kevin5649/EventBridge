using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBridge.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("EventPlanner")]
        public string PlannerId { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(2000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Booking Booking { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
        public virtual EventPlanner EventPlanner { get; set; } = null!;
    }
}
