using EventBridge.Interfaces;
using EventBridge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventBridge.Controllers
{
    [Authorize(Roles = "Customer,EventPlanner")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService,
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _userManager = userManager;
            _configuration = configuration;
        }


        // =========================================================
        // PAYMENT HISTORY
        // Customer   Their payments
        // Planner    Payments received for their bookings
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            IEnumerable<Payment> payments;

            if (User.IsInRole("Customer"))
            {
                payments = await _paymentService
                    .GetPaymentsByCustomerAsync(user.Id);
            }
            else if (User.IsInRole("EventPlanner"))
            {
                payments = await _paymentService
                    .GetPaymentsByPlannerAsync(user.Id);
            }
            else
            {
                return Forbid();
            }

            return View(payments);
        }


        // =========================================================
        // INITIATE PAYMENT
        // CUSTOMER ONLY
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Initiate(
            int bookingId,
            bool isAdvance)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var booking =
                await _bookingService.GetBookingByIdAsync(bookingId);

            if (booking == null ||
                booking.Quotation?.Enquiry?.CustomerId != user.Id)
            {
                return NotFound();
            }

            if (booking.Status == "Completed" ||
                booking.Status == "Cancelled")
            {
                return BadRequest(
                    "Cannot initiate payment for this booking.");
            }


            // =====================================================
            // DETERMINE PAYMENT TYPE
            // =====================================================

            string paymentType =
                isAdvance ? "Advance" : "Remaining";


            // =====================================================
            // CHECK EXISTING PAYMENTS
            // =====================================================

            var successfulPayments = booking.Payments?
                .Where(p => p.Status == "Success")
                .ToList()
                ?? new List<Payment>();

            var paidAmount =
                successfulPayments.Sum(p => p.Amount);


            // =====================================================
            // DETERMINE AMOUNT
            // =====================================================

            decimal amountToPay;

            if (paymentType == "Advance")
            {
                // Advance can only be paid once.
                if (successfulPayments.Any(
                    p => p.PaymentType == "Advance"))
                {
                    return BadRequest(
                        "Advance payment has already been completed.");
                }

                amountToPay = booking.AdvanceAmount;
            }
            else
            {
                // Remaining payment requires advance payment first.
                if (!successfulPayments.Any(
                    p => p.PaymentType == "Advance"))
                {
                    return BadRequest(
                        "Please pay the advance amount first.");
                }

                amountToPay =
                    booking.TotalAmount - paidAmount;

                if (amountToPay <= 0)
                {
                    return BadRequest(
                        "No remaining payment is required.");
                }
            }


            if (amountToPay <= 0)
                return BadRequest("Invalid payment amount.");


            // =====================================================
            // CREATE RAZORPAY ORDER
            // =====================================================

            var orderId =
                _paymentService.CreateRazorpayOrder(
                    amountToPay,
                    bookingId.ToString());


            ViewBag.OrderId = orderId;
            ViewBag.Amount = amountToPay;
            ViewBag.BookingId = bookingId;
            ViewBag.PaymentType = paymentType;


            // Only PUBLIC Razorpay Key ID goes to the view.
            // Key Secret remains on the server.
            ViewBag.RazorpayKeyId =
                _configuration["RazorpaySettings:KeyId"];


            return View(booking);
        }


        // =========================================================
        // PAYMENT SUCCESS
        // CUSTOMER ONLY
        // =========================================================

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Success(
            int bookingId,
            string transactionId,
            decimal amount,
            string paymentType,
            string razorpayOrderId,
            string razorpaySignature)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");


            var booking =
                await _bookingService.GetBookingByIdAsync(bookingId);

            if (booking == null ||
                booking.Quotation?.Enquiry?.CustomerId != user.Id)
            {
                return NotFound();
            }


            // =====================================================
            // VERIFY RAZORPAY RESPONSE
            // =====================================================

            if (string.IsNullOrWhiteSpace(razorpayOrderId) ||
                string.IsNullOrWhiteSpace(transactionId) ||
                string.IsNullOrWhiteSpace(razorpaySignature))
            {
                TempData["ErrorMessage"] =
                    "Invalid Razorpay payment response.";

                return RedirectToAction(
                    "Details",
                    "Booking",
                    new { id = bookingId });
            }


            // =====================================================
            // VERIFY RAZORPAY SIGNATURE
            // =====================================================

            var signatureValid =
                _paymentService.VerifyRazorpaySignature(
                    razorpayOrderId,
                    transactionId,
                    razorpaySignature);


            if (!signatureValid)
            {
                TempData["ErrorMessage"] =
                    "Payment verification failed.";

                return RedirectToAction(
                    "Details",
                    "Booking",
                    new { id = bookingId });
            }


            // =====================================================
            // PROCESS VERIFIED PAYMENT
            // =====================================================

            var success =
                await _paymentService.ProcessPaymentSuccessAsync(
                    bookingId,
                    transactionId,
                    amount,
                    paymentType);


            if (success)
            {
                TempData["SuccessMessage"] =
                    $"{paymentType} payment successful!";

                return RedirectToAction(
                    "Details",
                    "Booking",
                    new { id = bookingId });
            }


            TempData["ErrorMessage"] =
                "Payment processing failed.";

            return RedirectToAction(
                "Details",
                "Booking",
                new { id = bookingId });
        }


        // =========================================================
        // PAYMENT FAILURE NOTIFICATION
        // CUSTOMER ONLY
        // =========================================================

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PaymentFailed(
            [FromBody] PaymentFailureRequest request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var booking =
                await _bookingService.GetBookingByIdAsync(request.BookingId);

            if (booking == null ||
                booking.Quotation?.Enquiry?.CustomerId != user.Id)
            {
                return NotFound();
            }

            var paymentRecorded =
    await _paymentService.ProcessPaymentFailureAsync(
        request.BookingId,
        request.TransactionId ?? "",
        request.Amount,
        request.PaymentType);

            if (paymentRecorded)
            {
                await _paymentService.SendPaymentFailureEmailAsync(
                    request.BookingId,
                    request.Amount,
                    request.PaymentType,
                    request.ErrorDescription);
            }

            return Ok();
        }


        public class PaymentFailureRequest
        {
            public int BookingId { get; set; }

            public decimal Amount { get; set; }

            public string PaymentType { get; set; } = string.Empty;

            public string? TransactionId { get; set; }

            public string ErrorDescription { get; set; } = string.Empty;
        }
    }
}