using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBridge.Models
{
    public class Customer
    {
        [Key]
        [ForeignKey("User")]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;
    }
}
