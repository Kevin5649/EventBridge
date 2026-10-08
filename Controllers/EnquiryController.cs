using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventBridge.Controllers
{
    [Authorize]
    public class EnquiryController : Controller
    {
        private readonly IEnquiryService _enquiryService;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<EventPlanner> _plannerRepo;
        private readonly IRepository<PlannerEventType> _plannerEventTypeRepo;

        public EnquiryController(
            IEnquiryService enquiryService,
            UserManager<User> userManager,
            IRepository<EventPlanner> plannerRepo,
            IRepository<PlannerEventType> plannerEventTypeRepo)
        {
            _enquiryService = enquiryService;
            _userManager = userManager;
            _plannerRepo = plannerRepo;
            _plannerEventTypeRepo = plannerEventTypeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            IEnumerable<Enquiry> enquiries;

            if (user.Role == "Customer")
            {
                enquiries = await _enquiryService
                    .GetEnquiriesByCustomerAsync(user.Id);
            }
            else if (user.Role == "EventPlanner")
            {
                enquiries = await _enquiryService
                    .GetEnquiriesByPlannerAsync(user.Id);
            }
            else
            {
                return Forbid();
            }

            return View(enquiries.OrderByDescending(e => e.Id));
        }


        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Create(string plannerId)
        {
            var planners = await _plannerRepo.FindAsync(
                p => p.PlannerId == plannerId);

            var planner = planners.FirstOrDefault();

            if (planner == null)
                return NotFound();


            var plannerEventTypes =
                await _plannerEventTypeRepo.FindAsync(
                    p => p.PlannerId == plannerId,
                    "Category");


            var model = new EnquiryCreateViewModel
            {
                PlannerId = plannerId,
                PlannerName = planner.CompanyName,

                Categories = plannerEventTypes
                    .Where(x => x.Category != null)
                    .Select(x => new SelectListItem
                    {
                        Value = x.CategoryId.ToString(),
                        Text = x.Category.Name
                    })
                    .ToList()
            };


            return View(model);
        }


        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EnquiryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories(model);
                return View(model);
            }


            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");


            var success = await _enquiryService
                .CreateEnquiryAsync(user.Id, model);


            if (success)
            {
                TempData["SuccessMessage"] =
                    "Enquiry sent successfully.";

                return RedirectToAction(nameof(Index));
            }


            await LoadCategories(model);

            ModelState.AddModelError(
                "",
                "Failed to send enquiry.");

            return View(model);
        }


        private async Task LoadCategories(
            EnquiryCreateViewModel model)
        {
            var plannerEventTypes =
                await _plannerEventTypeRepo.FindAsync(
                    p => p.PlannerId == model.PlannerId,
                    "Category");


            model.Categories = plannerEventTypes
                .Where(x => x.Category != null)
                .Select(x => new SelectListItem
                {
                    Value = x.CategoryId.ToString(),
                    Text = x.Category.Name
                })
                .ToList();
        }


        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");


            var enquiry =
                await _enquiryService.GetEnquiryByIdAsync(id);

            if (enquiry == null)
                return NotFound();


            if (user.Role == "Customer" &&
                enquiry.CustomerId != user.Id)
                return Forbid();


            if (user.Role == "EventPlanner" &&
                enquiry.PlannerId != user.Id)
                return Forbid();


            return View(enquiry);
        }
    }
}