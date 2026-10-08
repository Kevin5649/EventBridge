using EventBridge.ViewModels;

namespace EventBridge.Interfaces
{
    public interface IPlannerSearchService
    {
        Task<PlannerSearchViewModel> SearchPlannersAsync(PlannerSearchViewModel searchParams);
    }
}
