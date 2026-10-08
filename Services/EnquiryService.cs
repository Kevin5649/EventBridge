using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;

namespace EventBridge.Services
{
    public class EnquiryService : IEnquiryService
    {
        private readonly IRepository<Enquiry> _enquiryRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IRepository<EventPlanner> _plannerRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnquiryService(
            IRepository<Enquiry> enquiryRepo,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IRepository<EventPlanner> plannerRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _enquiryRepo = enquiryRepo;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _plannerRepo = plannerRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CreateEnquiryAsync(
            string customerId,
            EnquiryCreateViewModel model)
        {
            var enquiry = new Enquiry
            {
                CustomerId = customerId,
                PlannerId = model.PlannerId,
                CategoryId = model.CategoryId,

                EventDate = model.EventDate,
                Budget = model.Budget,
                GuestCount = model.GuestCount,
                Venue = model.Venue,
                RequiredServices = model.RequiredServices,
                Message = model.Message,

                Status = "Pending"
            };

            await _enquiryRepo.AddAsync(enquiry);
            await _unitOfWork.SaveChangesAsync();

            // Send Email to Planner
            var loginUrl =
    $"{_httpContextAccessor.HttpContext!.Request.Scheme}://" +
    $"{_httpContextAccessor.HttpContext.Request.Host}" +
    "/Account/Login";

            var planners = await _plannerRepo.FindAsync(
                p => p.PlannerId == model.PlannerId,
                includeProperties: "User");

            var planner = planners.FirstOrDefault();

            if (planner != null && planner.User != null)
            {
                await _emailService.SendEmailAsync(
                    planner.User.Email!,
                    "New Enquiry Received - EventBridge",
                    $@"
        <div style='font-family:Arial,sans-serif; max-width:600px; margin:auto;'>
            
            <h2 style='color:#52796f;'>
                EventBridge - New Enquiry
            </h2>

            <p>
                You have received a new enquiry on EventBridge.
            </p>

            <p>
                Please log in to your EventBridge account to view the
                enquiry details.
            </p>

            <div style='text-align:center; margin:30px 0;'>
                <a href='{loginUrl}'
                   style='
                   background:#52796f;
                   color:white;
                   padding:12px 24px;
                   text-decoration:none;
                   border-radius:8px;
                   display:inline-block;
                   font-weight:bold;'>
                    Login to EventBridge
                </a>
            </div>

            <p style='color:#777; font-size:13px;'>
                This is an automated email from EventBridge.
            </p>

        </div>");
            }
            return true;
        }

        public async Task<IEnumerable<Enquiry>>
            GetEnquiriesByCustomerAsync(string customerId)
        {
            return await _enquiryRepo.FindAsync(
                e => e.CustomerId == customerId,
                "EventPlanner.User,Category,Quotation");
        }

        public async Task<IEnumerable<Enquiry>>
            GetEnquiriesByPlannerAsync(string plannerId)
        {
            return await _enquiryRepo.FindAsync(
                e => e.PlannerId == plannerId,
                "Customer.User,Category,Quotation");
        }

        public async Task<Enquiry?> GetEnquiryByIdAsync(int id)
        {
            var enquiries = await _enquiryRepo.FindAsync(
                e => e.Id == id,
                "Customer.User,EventPlanner.User,Category,Quotation");

            return enquiries.FirstOrDefault();
        }

        public async Task<bool> UpdateEnquiryStatusAsync(
            int enquiryId,
            string status)
        {
            var enquiry = await GetEnquiryByIdAsync(enquiryId);

            if (enquiry == null)
                return false;

            enquiry.Status = status;

            _enquiryRepo.Update(enquiry);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}