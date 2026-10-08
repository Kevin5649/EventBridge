using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventBridge.ViewModels
{
    public class EnquiryCreateViewModel
    {
        [Required]
        public string PlannerId { get; set; } = string.Empty;

        public string PlannerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an event type.")]
        [Display(Name = "Event Type")]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
            = new List<SelectListItem>();

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Budget must be greater than zero.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Budget")]
        public decimal Budget { get; set; }

        [Required]
        [Range(1, 100000, ErrorMessage = "Guest count must be at least 1.")]
        [Display(Name = "Expected Guests")]
        public int GuestCount { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Venue / Location")]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Required Services")]
        public string RequiredServices { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Message / Requirements")]
        public string Message { get; set; } = string.Empty;
    }
}