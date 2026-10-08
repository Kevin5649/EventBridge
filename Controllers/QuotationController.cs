using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    [Authorize]
    public class QuotationController : Controller
    {
        private readonly IQuotationService _quotationService;
        private readonly IEnquiryService _enquiryService;
        private readonly UserManager<User> _userManager;

        public QuotationController(IQuotationService quotationService, IEnquiryService enquiryService, UserManager<User> userManager)
        {
            _quotationService = quotationService;
            _enquiryService = enquiryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var quotation = await _quotationService.GetQuotationByIdAsync(id);
            if (quotation == null) return NotFound();

            if (user.Role == "Customer" && quotation.Enquiry?.CustomerId != user.Id) return Forbid();
            if (user.Role == "EventPlanner" && quotation.Enquiry?.PlannerId != user.Id) return Forbid();

            return View(quotation);
        }

        [Authorize(Roles = "EventPlanner")]
        [HttpGet]
        public async Task<IActionResult> Create(int enquiryId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var enquiry = await _enquiryService.GetEnquiryByIdAsync(enquiryId);
            if (enquiry == null || enquiry.PlannerId != user.Id || enquiry.Status != "Pending") 
            {
                return BadRequest("Cannot create quotation for this enquiry.");
            }

            // Check if quote already exists
            var existingQuote = await _quotationService.GetQuotationByEnquiryAsync(enquiryId);
            if (existingQuote != null) return RedirectToAction(nameof(Details), new { id = existingQuote.Id });

            var model = new QuotationCreateViewModel
            {
                EnquiryId = enquiryId
            };

            return View(model);
        }

        [Authorize(Roles = "EventPlanner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuotationCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var enquiry = await _enquiryService.GetEnquiryByIdAsync(model.EnquiryId);
            if (enquiry == null || enquiry.PlannerId != user.Id) return Forbid();

            var success = await _quotationService.CreateQuotationAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "Quotation sent successfully.";
                return RedirectToAction("Index", "Enquiry");
            }

            ModelState.AddModelError("", "Failed to create quotation.");
            return View(model);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var success = await _quotationService.AcceptQuotationAsync(id, user.Id);
            if (success)
            {
                TempData["SuccessMessage"] = "Quotation accepted! A booking has been created.";
                return RedirectToAction("Index", "Booking");
            }

            TempData["ErrorMessage"] = "Failed to accept quotation.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var success = await _quotationService.RejectQuotationAsync(id, user.Id);
            if (success)
            {
                TempData["SuccessMessage"] = "Quotation rejected.";
                return RedirectToAction("Index", "Enquiry");
            }

            TempData["ErrorMessage"] = "Failed to reject quotation.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
