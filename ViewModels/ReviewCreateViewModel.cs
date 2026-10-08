using System.ComponentModel.DataAnnotations;

namespace EventBridge.ViewModels
{
    public class ReviewCreateViewModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(2000)]
        public string? Comment { get; set; }
    }
}
