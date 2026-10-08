using EventBridge.Interfaces;
using EventBridge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<User> _userManager;

        public AdminController(
            IAdminService adminService,
            UserManager<User> userManager)
        {
            _adminService = adminService;
            _userManager = userManager;
        }

        // =========================
        // VERIFY PLANNER
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPlanner(
            string id,
            string returnPage)
        {
            var success = await _adminService.VerifyPlannerAsync(
                id,
                "Verified by Admin"
            );

            if (success)
            {
                TempData["SuccessMessage"] =
                    "Planner verified successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Verification failed.";
            }

            if (returnPage == "Planners")
            {
                return RedirectToAction("Planners", "Admin");
            }

            if (returnPage == "AdminIndex")
            {
                return RedirectToAction("AdminIndex", "Admin");
            }

            return RedirectToAction("PendingPlanners", "Admin");
        }

        // =========================
        // UNVERIFY PLANNER
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnverifyPlanner(string id)
        {
            var success = await _adminService.UnverifyPlannerAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] =
                    "Planner unverified successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Unverification failed.";
            }

            return RedirectToAction("Planners", "Admin");
        }

        // =========================
        // REJECT PLANNER
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPlanner(
            string id,
            string returnPage)
        {
            var success = await _adminService.RejectPlannerAsync(
                id,
                "Rejected by Admin"
            );

            if (success)
            {
                TempData["SuccessMessage"] =
                    "Planner rejected.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Rejection failed.";
            }

            if (returnPage == "Planners")
            {
                return RedirectToAction("Planners", "Admin");
            }

            if (returnPage == "AdminIndex")
            {
                return RedirectToAction("AdminIndex", "Dashboard");
            }

            return RedirectToAction("PendingPlanners", "Admin");
        }


        // =========================
        // ACTIVATE / DEACTIVATE USER
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(
            string id,
            bool activate,
            string returnPage)
        {
            bool success = activate
                ? await _adminService.ActivateUserAsync(id)
                : await _adminService.DeactivateUserAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] =
                    $"User {(activate ? "activated" : "deactivated")} successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Status update failed.";
            }

            if (returnPage == "Planners")
            {
                return RedirectToAction("Planners", "Admin");
            }

            return RedirectToAction("Customers", "Admin");
        }


        // =========================
        // MANAGE CUSTOMERS
        // =========================
        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var customers = await _adminService.GetAllCustomersAsync();

            return View("~/Views/Dashboard/Customers.cshtml", customers);
        }


        // =========================
        // MANAGE PLANNERS
        // =========================
        [HttpGet]
        public async Task<IActionResult> Planners()
        {
            var planners = await _adminService.GetAllPlannersAsync();

            return View("~/Views/Dashboard/Planners.cshtml", planners);
        }


        // =========================
        // PENDING PLANNER VERIFICATIONS
        // =========================
        [HttpGet]
        public async Task<IActionResult> PendingPlanners()
        {
            var planners = await _adminService.GetAllPlannersAsync();

            var pendingPlanners = planners
                .Where(p => !p.IsVerified)
                .OrderByDescending(p => p.AvgRating)
                .ThenByDescending(p => p.ReviewCount)
                .ToList();

            return View(
                "~/Views/Dashboard/PendingPlanners.cshtml",
                pendingPlanners
            );
        }
    }
}