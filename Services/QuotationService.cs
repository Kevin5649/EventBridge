using EventBridge.Interfaces;
using EventBridge.Models;
using EventBridge.ViewModels;

namespace EventBridge.Services
{
    public class QuotationService : IQuotationService
    {
        private readonly IRepository<Quotation> _quotationRepo;
        private readonly IEnquiryService _enquiryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuotationService(
            IRepository<Quotation> quotationRepo,
            IEnquiryService enquiryService,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IRepository<Booking> bookingRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _quotationRepo = quotationRepo;
            _enquiryService = enquiryService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _bookingRepo = bookingRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> AcceptQuotationAsync(
     int quotationId,
     string customerId)
        {
            var quote = await GetQuotationByIdAsync(quotationId);

            if (quote == null ||
                quote.Enquiry?.CustomerId != customerId)
                return false;

            quote.Status = "Accepted";
            _quotationRepo.Update(quote);

            await _enquiryService.UpdateEnquiryStatusAsync(
                quote.EnquiryId,
                "Accepted");

            var booking = new Booking
            {
                CustomerId = quote.Enquiry.CustomerId,
                PlannerId = quote.Enquiry.PlannerId,
                QuotationId = quotationId,
                TotalAmount = quote.Amount,
                AdvanceAmount = quote.Amount * 0.2m,
                Status = "Upcoming",
                BookedAt = DateTime.Now
            };

            await _bookingRepo.AddAsync(booking);

            await _unitOfWork.SaveChangesAsync();


            // =========================================
            // DYNAMIC LOGIN URL
            // =========================================

            var loginUrl =
                $"{_httpContextAccessor.HttpContext!.Request.Scheme}://" +
                $"{_httpContextAccessor.HttpContext.Request.Host}" +
                "/Account/Login";


            // =========================================
            // BOOKING CONFIRMED EMAIL - CUSTOMER
            // =========================================

            if (quote.Enquiry?.Customer?.User?.Email != null &&
                quote.Enquiry?.EventPlanner?.CompanyName != null)
            {
                await _emailService.SendEmailAsync(
                    quote.Enquiry.Customer.User.Email,
                    "Booking Confirmed - EventBridge",
                    $@"
            <!DOCTYPE html>
            <html>
            <body style='margin:0; padding:0; background:#f4f7f6; font-family:Arial,sans-serif;'>

                <div style='max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; padding:40px; text-align:center; box-shadow:0 4px 20px rgba(0,0,0,0.08);'>

                    <h1 style='color:#52796f; margin-bottom:10px;'>
                        EventBridge
                    </h1>

                    <h2 style='color:#263238;'>
                        Booking Confirmed
                    </h2>

                    <p style='color:#607d8b; font-size:15px; line-height:1.6;'>
                        Your booking with
                        <strong>{quote.Enquiry.EventPlanner.CompanyName}</strong>
                        has been confirmed successfully.
                    </p>

                    <div style='background:#f4f7f6; border-radius:8px; padding:18px; margin:25px 0; text-align:left;'>

                        <p style='margin:8px 0; color:#455a64;'>
                            <strong>Total Amount:</strong>
                            INR {quote.Amount:N2}
                        </p>

                        <p style='margin:8px 0; color:#455a64;'>
                            <strong>Advance Amount:</strong>
                            INR {booking.AdvanceAmount:N2}
                        </p>

                        <p style='margin:8px 0; color:#455a64;'>
                            <strong>Booking Status:</strong>
                            {booking.Status}
                        </p>

                    </div>

                    <p style='color:#607d8b; font-size:15px; line-height:1.6;'>
                        Please log in to EventBridge to view your booking details.
                    </p>

                    <div style='margin:30px 0;'>

                        <a href='{loginUrl}'
                           style='display:inline-block; background:#52796f; color:#ffffff; text-decoration:none; padding:14px 28px; border-radius:7px; font-size:16px; font-weight:bold;'>
                            Login to EventBridge
                        </a>

                    </div>

                    <p style='color:#9aa5a1; font-size:12px; margin-top:30px;'>
                        This is an automated email from EventBridge.
                    </p>

                </div>

            </body>
            </html>
            ");
            }


            // =========================================
            // BOOKING CONFIRMED EMAIL - PLANNER
            // =========================================

            if (quote.Enquiry?.EventPlanner?.User?.Email != null)
            {
                var customerName =
                    quote.Enquiry.Customer?.User?.UserName
                    ?? "A customer";

                await _emailService.SendEmailAsync(
                    quote.Enquiry.EventPlanner.User.Email,
                    "New Booking Confirmed - EventBridge",
                    $@"
            <!DOCTYPE html>
            <html>
            <body style='margin:0; padding:0; background:#f4f7f6; font-family:Arial,sans-serif;'>

                <div style='max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; padding:40px; text-align:center; box-shadow:0 4px 20px rgba(0,0,0,0.08);'>

                    <h1 style='color:#52796f; margin-bottom:10px;'>
                        EventBridge
                    </h1>

                    <h2 style='color:#263238;'>
                        New Booking Confirmed
                    </h2>

                    <p style='color:#607d8b; font-size:15px; line-height:1.6;'>
                        <strong>{customerName}</strong> has accepted your quotation
                        and a new booking has been confirmed.
                    </p>

                    <div style='background:#f4f7f6; border-radius:8px; padding:18px; margin:25px 0; text-align:left;'>

                        <p style='margin:8px 0; color:#455a64;'>
                            <strong>Total Amount:</strong>
                            INR {quote.Amount:N2}
                        </p>

                        <p style='margin:8px 0; color:#455a64;'>
                            <strong>Advance Amount:</strong>
                            INR {booking.AdvanceAmount:N2}
                        </p>

                        <p style='margin:8px 0; color:#455a64;'>
                            <strong>Booking Status:</strong>
                            {booking.Status}
                        </p>

                    </div>

                    <p style='color:#607d8b; font-size:15px; line-height:1.6;'>
                        Please log in to EventBridge to view your booking details.
                    </p>

                    <div style='margin:30px 0;'>

                        <a href='{loginUrl}'
                           style='display:inline-block; background:#52796f; color:#ffffff; text-decoration:none; padding:14px 28px; border-radius:7px; font-size:16px; font-weight:bold;'>
                            Login to EventBridge
                        </a>

                    </div>

                    <p style='color:#9aa5a1; font-size:12px; margin-top:30px;'>
                        This is an automated email from EventBridge.
                    </p>

                </div>

            </body>
            </html>
            ");
            }

            return true;
        }
        public async Task<bool> CreateQuotationAsync(
            QuotationCreateViewModel model)
        {
            var enquiry =
                await _enquiryService.GetEnquiryByIdAsync(
                    model.EnquiryId);

            if (enquiry == null)
                return false;

            var existingQuote =
                await GetQuotationByEnquiryAsync(
                    model.EnquiryId);

            if (existingQuote != null)
                return false;

            var quotation = new Quotation
            {
                EnquiryId = model.EnquiryId,
                Amount = model.Amount,

                ServicesIncluded = model.ServicesIncluded,
                Description = model.Description,
                TermsAndNotes = model.TermsAndNotes,

                ValidTill = DateTime.Today.AddDays(7),
                Status = "Pending"
            };

            await _quotationRepo.AddAsync(quotation);

            await _enquiryService.UpdateEnquiryStatusAsync(
                model.EnquiryId,
                "Quoted");

            await _unitOfWork.SaveChangesAsync();


            // =========================================
            // SEND EMAIL TO CUSTOMER
            // =========================================

            if (enquiry.Customer?.User?.Email != null &&
                enquiry.EventPlanner?.CompanyName != null)
            {
                var loginUrl =
                    $"{_httpContextAccessor.HttpContext!.Request.Scheme}://" +
                    $"{_httpContextAccessor.HttpContext.Request.Host}" +
                    "/Account/Login";

                await _emailService.SendEmailAsync(
                    enquiry.Customer.User.Email,
                    "New Quotation Received - EventBridge",
                    $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='margin:0; padding:0; background:#f4f7f6; font-family:Arial,sans-serif;'>

                        <div style='max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; padding:40px; text-align:center; box-shadow:0 4px 20px rgba(0,0,0,0.08);'>

                            <h1 style='color:#52796f; margin-bottom:10px;'>
                                EventBridge
                            </h1>

                            <h2 style='color:#263238;'>
                                New Quotation Received
                            </h2>

                            <p style='color:#607d8b; font-size:15px; line-height:1.6;'>
                                You have received a new quotation from
                                <strong>{enquiry.EventPlanner.CompanyName}</strong>.
                            </p>

                            <div style='background:#f4f7f6; border-radius:8px; padding:18px; margin:25px 0; text-align:left;'>

                                <p style='margin:8px 0; color:#455a64;'>
                                    <strong>Quotation Amount:</strong>
                                    INR {model.Amount:N2}
                                </p>

                                <p style='margin:8px 0; color:#455a64;'>
                                    <strong>Valid Until:</strong>
                                    {DateTime.Today.AddDays(7):dd/MM/yyyy}
                                </p>

                            </div>

                            <p style='color:#607d8b; font-size:15px; line-height:1.6;'>
                                Please log in to EventBridge to view the
                                complete quotation details.
                            </p>

                            <div style='margin:30px 0;'>

                                <a href='{loginUrl}'
                                   style='display:inline-block; background:#52796f; color:#ffffff; text-decoration:none; padding:14px 28px; border-radius:7px; font-size:16px; font-weight:bold;'>

                                    Login to EventBridge

                                </a>

                            </div>

                            <p style='color:#9aa5a1; font-size:12px; margin-top:30px;'>
                                This is an automated email from EventBridge.
                            </p>

                        </div>

                    </body>
                    </html>
                    ");
            }

            return true;
        }

        public async Task<Quotation?>
            GetQuotationByEnquiryAsync(int enquiryId)
        {
            var quotes = await _quotationRepo.FindAsync(
                q => q.EnquiryId == enquiryId,
                "Enquiry.Customer.User,Enquiry.EventPlanner.User,Enquiry.Category");

            return quotes.FirstOrDefault();
        }

        public async Task<Quotation?>
            GetQuotationByIdAsync(int id)
        {
            var quotes = await _quotationRepo.FindAsync(
                q => q.Id == id,
                "Enquiry.Customer.User,Enquiry.EventPlanner.User,Enquiry.Category");

            return quotes.FirstOrDefault();
        }

        public async Task<bool> RejectQuotationAsync(
            int quotationId,
            string customerId)
        {
            var quote =
                await GetQuotationByIdAsync(quotationId);

            if (quote == null ||
                quote.Enquiry?.CustomerId != customerId)
                return false;

            quote.Status = "Rejected";
            _quotationRepo.Update(quote);

            await _enquiryService.UpdateEnquiryStatusAsync(
                quote.EnquiryId,
                "Rejected");

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}