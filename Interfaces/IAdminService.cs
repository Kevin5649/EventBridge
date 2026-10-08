using EventBridge.Models;
using EventBridge.ViewModels;

namespace EventBridge.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardStatsAsync();

        Task<bool> VerifyPlannerAsync(string plannerId, string remarks);

        Task<bool> UnverifyPlannerAsync(string plannerId);

        Task<bool> RejectPlannerAsync(string plannerId, string remarks);

        Task<bool> ActivateUserAsync(string userId);

        Task<bool> DeactivateUserAsync(string userId);

        // Admin user management
        Task<IEnumerable<Customer>> GetAllCustomersAsync();

        Task<IEnumerable<EventPlanner>> GetAllPlannersAsync();
    }
}