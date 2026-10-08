using System.ComponentModel.DataAnnotations;

namespace EventBridge.ViewModels
{
    public class QuotationCreateViewModel
    {
        [Required]
        public int EnquiryId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        [Display(Name = "Quotation Amount")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Services Included")]
        public string ServicesIncluded { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Service Details / Proposal")]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Terms & Additional Notes")]
        public string TermsAndNotes { get; set; } = string.Empty;
    }
}