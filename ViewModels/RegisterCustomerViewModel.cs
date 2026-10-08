using System.ComponentModel.DataAnnotations;

namespace EventBridge.ViewModels
{
    public class RegisterCustomerViewModel
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
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
    }
}