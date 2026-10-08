using EventBridge.Models;
using EventBridge.ViewModels;

namespace EventBridge.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetReviewsByPlannerAsync(string plannerId);
        Task<bool> CreateReviewAsync(string customerId, ReviewCreateViewModel model);
        Task<bool> CanReviewAsync(int bookingId, string customerId);
    }
}
