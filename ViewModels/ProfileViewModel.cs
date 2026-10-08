using Microsoft.AspNetCore.Http;

namespace EventBridge.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public IFormFile? ProfilePicture { get; set; }

        // Customer Specific
        public string? Address { get; set; }

        // Planner Specific
        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public int? Experience { get; set; }
    }
}
