using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventBridge.ViewModels
{
    public class RegisterPlannerViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [RegularExpression(
        @"^[^@\s]+@(gmail\.com|outlook\.com|hotmail\.com|yahoo\.com|icloud\.com)$",
        ErrorMessage = "Please use a Gmail, Outlook, Hotmail, Yahoo, or iCloud email address.")]
        public string Email { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Country Code")]
        public string CountryCode { get; set; } = "+91";

        [Required]
        [RegularExpression(@"^[1-9][0-9]{9}$",
            ErrorMessage = "Phone number must be exactly 10 digits and cannot start with 0.")]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;


        [Required]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one number and one special character.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password",
            ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;


        [Required]
        [Range(0, 50)]
        [Display(Name = "Experience (Years)")]
        public int Experience { get; set; }


        [Required(ErrorMessage = "Please select at least one event type.")]
        [MinLength(1, ErrorMessage = "Please select at least one event type.")]
        [Display(Name = "Event Types")]
        public List<int> SelectedCategoryIds { get; set; } = new();


        public IEnumerable<SelectListItem> Categories { get; set; }
            = new List<SelectListItem>();
    }
}