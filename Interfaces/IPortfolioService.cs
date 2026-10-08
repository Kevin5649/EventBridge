using EventBridge.Models;

namespace EventBridge.Interfaces
{
    public interface IPortfolioService
    {
        Task<IEnumerable<PlannerPortfolio>> GetPortfolioAsync(string plannerId);
        Task<bool> AddImageAsync(string plannerId, string imagePath, string caption);
        Task<bool> DeleteImageAsync(int portfolioId, string plannerId);
        Task<int> GetPortfolioCountAsync(string plannerId);
    }
}
