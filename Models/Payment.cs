using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBridge.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "Razorpay";

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Success";

        [Required]
        [StringLength(20)]
        public string PaymentType { get; set; } = "Advance";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public virtual Booking Booking { get; set; } = null!;
    }
}
