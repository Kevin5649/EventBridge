using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EventBridge.ViewModels
{
    public class PortfolioUploadViewModel
    {
        [Required(ErrorMessage = "Please select an image.")]
        public IFormFile Image { get; set; } = null!;

        [StringLength(255)]
        public string? Caption { get; set; }
    }
}
