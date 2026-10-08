using EventBridge.Interfaces;
using EventBridge.Models;

namespace EventBridge.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingService(
            IRepository<Booking> bookingRepo,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _bookingRepo = bookingRepo;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            var bookings = await _bookingRepo.FindAsync(
                b => b.Id == id,
                "Quotation.Enquiry.Customer.User,Quotation.Enquiry.EventPlanner.User,Payments,Review");

            return bookings.FirstOrDefault();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByCustomerAsync(
            string customerId)
        {
            return await _bookingRepo.FindAsync(
                b => b.Quotation != null &&
                     b.Quotation.Enquiry != null &&
                     b.Quotation.Enquiry.CustomerId == customerId,
                "Quotation.Enquiry.EventPlanner.User,Payments");
        }

        public async Task<IEnumerable<Booking>> GetBookingsByPlannerAsync(
            string plannerId)
        {
            return await _bookingRepo.FindAsync(
                b => b.Quotation != null &&
                     b.Quotation.Enquiry != null &&
                     b.Quotation.Enquiry.PlannerId == plannerId,
                "Quotation.Enquiry.Customer.User,Payments");
        }

        public async Task<bool> UpdateBookingStatusAsync(
            int bookingId,
            string status,
            string plannerId)
        {
            var booking = await GetBookingByIdAsync(bookingId);

            if (booking == null ||
                booking.Quotation?.Enquiry?.PlannerId != plannerId)
                return false;

            // =========================================
            // PREVENT COMPLETION BEFORE FULL PAYMENT
            // =========================================

            if (status == "Completed")
            {
                var paidAmount = booking.Payments?
                    .Where(p => p.Status == "Success")
                    .Sum(p => p.Amount) ?? 0;

                if (paidAmount < booking.TotalAmount)
                {
                    return false;
                }
            }

            booking.Status = status;

            _bookingRepo.Update(booking);

            await _unitOfWork.SaveChangesAsync();


            // =========================================
            // EVENT COMPLETED EMAIL
            // =========================================

            if (status == "Completed" &&
                booking.Quotation?.Enquiry?.Customer?.User?.Email != null)
            {
                var loginUrl =
                    $"{_httpContextAccessor.HttpContext!.Request.Scheme}://" +
                    $"{_httpContextAccessor.HttpContext.Request.Host}" +
                    "/Account/Login";

                await _emailService.SendEmailAsync(
                    booking.Quotation.Enquiry.Customer.User.Email,
                    "Event Completed - Leave a Review",
                    $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='margin:0; padding:0; background:#f4f7f6; font-family:Arial,sans-serif;'>

                        <div style='max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; padding:40px; text-align:center; box-shadow:0 4px 20px rgba(0,0,0,0.08);'>

                            <h1 style='color:#52796f; margin-bottom:10px;'>
                                EventBridge
                            </h1>

                            <h2 style='color:#263238;'>
                                Event Completed
                            </h2>

                            <p style='color:#607d8b; font-size:15px; line-height:1.6;'>
                                Your event has been marked as completed successfully.
                            </p>

                            <p style='color:#607d8b; font-size:15px; line-height:1.6;'>
                                We would love to hear about your experience.
                                Please log in to EventBridge to leave a review
                                for your event planner.
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
    }
}