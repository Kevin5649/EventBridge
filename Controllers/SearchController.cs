using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    public class SearchController : Controller
    {
        private readonly IPlannerSearchService _searchService;
        private readonly IPortfolioService _portfolioService;
        private readonly IRepository<EventPlanner> _plannerRepo;
        private readonly IReviewService _reviewService;
        private readonly IRepository<Category> _categoryRepo;

        public SearchController(
            IPlannerSearchService searchService,
            IPortfolioService portfolioService,
            IRepository<EventPlanner> plannerRepo,
            IReviewService reviewService,
            IRepository<Category> categoryRepo)
        {
            _searchService = searchService;
            _portfolioService = portfolioService;
            _plannerRepo = plannerRepo;
            _reviewService = reviewService;
            _categoryRepo = categoryRepo;
        }


        // =========================
        // SEARCH PLANNERS
        // =========================

        public async Task<IActionResult> Index(
            PlannerSearchViewModel model)
        {
            var categories =
                await _categoryRepo.GetAllAsync();

            ViewBag.Categories =
                categories
                    .OrderBy(c => c.Name)
                    .ToList();

            var result =
                await _searchService.SearchPlannersAsync(model);

            return View(result);
        }


        // =========================
        // PLANNER PROFILE
        // =========================

        public async Task<IActionResult> Profile(
            string id)
        {
            var planners =
                await _plannerRepo.FindAsync(
                    p => p.PlannerId == id,
                    "User");

            var planner =
                planners.FirstOrDefault();

            if (planner == null)
            {
                return NotFound();
            }

            var portfolio =
                await _portfolioService
                    .GetPortfolioAsync(id);

            var reviews =
                await _reviewService
                    .GetReviewsByPlannerAsync(id);

            ViewBag.Portfolio = portfolio;
            ViewBag.Reviews = reviews;

            return View(planner);
        }
    }
}