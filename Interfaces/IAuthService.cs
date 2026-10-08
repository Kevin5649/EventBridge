using EventBridge.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace EventBridge.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterCustomerAsync(RegisterCustomerViewModel model);

        Task<IdentityResult> RegisterPlannerAsync(RegisterPlannerViewModel model);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();

        Task<string> GeneratePasswordResetTokenAsync(string email);

        Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model);

        // =========================
        // EMAIL CONFIRMATION
        // =========================

        Task<string> GenerateEmailConfirmationTokenAsync(string email);
        Task<bool> IsEmailAlreadyConfirmedAsync(string email);

        Task<bool> ValidatePasswordResetTokenAsync(
        string email,
        string token);

        Task<IdentityResult> ConfirmEmailAsync(
        string email,
        string token);
    }
}