using EventBridge.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace EventBridge.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(string userId);
        Task<IdentityResult> UpdateProfileAsync(string userId, ProfileViewModel model);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
    }
}
