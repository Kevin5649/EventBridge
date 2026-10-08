using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBridge.Models
{
    public class Quotation
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Enquiry")]
        public int EnquiryId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string ServicesIncluded { get; set; } = string.Empty;

        [StringLength(1000)]
        public string TermsAndNotes { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime ValidTill { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Enquiry Enquiry { get; set; } = null!;
    }
}