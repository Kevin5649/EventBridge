using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBridge.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("EventPlanner")]
        public string PlannerId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Quotation")]
        public int QuotationId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceAmount { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Confirmed";

        public virtual Customer Customer { get; set; } = null!;
        public virtual EventPlanner EventPlanner { get; set; } = null!;
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; }
    = new List<Payment>();
        public virtual Review? Review { get; set; }
    }
}
