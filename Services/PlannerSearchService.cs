using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;

namespace EventBridge.Services
{
    public class PlannerSearchService : IPlannerSearchService
    {
        private readonly IRepository<EventPlanner> _plannerRepo;

        public PlannerSearchService(
            IRepository<EventPlanner> plannerRepo)
        {
            _plannerRepo = plannerRepo;
        }

        public async Task<PlannerSearchViewModel>
            SearchPlannersAsync(
                PlannerSearchViewModel searchParams)
        {
            var allPlanners =
                await _plannerRepo.GetAllAsync(
                    "User,PlannerEventTypes");

            var query = allPlanners.AsQueryable();


            // COMPANY NAME
            if (!string.IsNullOrEmpty(
                searchParams.CompanyName))
            {
                query = query.Where(
                    p => p.CompanyName.Contains(
                        searchParams.CompanyName,
                        StringComparison.OrdinalIgnoreCase));
            }


            // CATEGORY
            if (searchParams.CategoryId.HasValue)
            {
                query = query.Where(
                    p => p.PlannerEventTypes.Any(
                        pet => pet.CategoryId ==
                               searchParams.CategoryId.Value));
            }


            // EXPERIENCE
            if (searchParams.MinExperience.HasValue)
            {
                query = query.Where(
                    p => p.Experience >=
                         searchParams.MinExperience.Value);
            }


            // RATING
            if (searchParams.MinRating.HasValue)
            {
                query = query.Where(
                    p => p.AvgRating >=
                         searchParams.MinRating.Value);
            }


            // VERIFIED
            if (searchParams.OnlyVerified)
            {
                query = query.Where(
                    p => p.IsVerified);
            }


            searchParams.Planners =
                query.ToList();

            return searchParams;
        }
    }
}