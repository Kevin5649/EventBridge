using EventBridge.Configurations;
using EventBridge.Interfaces;
using EventBridge.Models;
using Microsoft.Extensions.Options;

namespace EventBridge.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly RazorpaySettings _razorpaySettings;

        public PaymentService(
            IRepository<Payment> paymentRepo,
            IRepository<Booking> bookingRepo,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IOptions<RazorpaySettings> razorpaySettings)
        {
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _razorpaySettings = razorpaySettings.Value;
        }


        // =========================================================
        // CREATE RAZORPAY ORDER
        // =========================================================

        public string CreateRazorpayOrder(
            decimal amount,
            string receiptId)
        {
            var client = new Razorpay.Api.RazorpayClient(
                _razorpaySettings.KeyId,
                _razorpaySettings.KeySecret);

            var options = new Dictionary<string, object>
            {
                { "amount", (int)(amount * 100) },
                { "receipt", receiptId },
                { "currency", "INR" }
            };

            var order = client.Order.Create(options);

            return order["id"].ToString();
        }


        // =========================================================
        // VERIFY RAZORPAY SIGNATURE
        // =========================================================

        public bool VerifyRazorpaySignature(
            string orderId,
            string paymentId,
            string signature)
        {
            try
            {
                var attributes = new Dictionary<string, string>
                {
                    { "razorpay_order_id", orderId },
                    { "razorpay_payment_id", paymentId },
                    { "razorpay_signature", signature }
                };

                Razorpay.Api.Utils.verifyPaymentSignature(attributes);

                return true;
            }
            catch
            {
                return false;
            }
        }


        // =========================================================
        // GET PAYMENT BY ID
        // =========================================================

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            var payments = await _paymentRepo.FindAsync(
                p => p.Id == id,
                "Booking.Quotation.Enquiry.Customer.User");

            return payments.FirstOrDefault();
        }


        // =========================================================
        // CUSTOMER PAYMENT HISTORY
        // =========================================================

        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(
            string customerId)
        {
            var payments = await _paymentRepo.FindAsync(
                p => p.Booking.CustomerId == customerId,
                "Booking.Quotation.Enquiry.EventPlanner.User");

            return payments.OrderByDescending(p => p.PaymentDate);
        }


        // =========================================================
        // PLANNER PAYMENT HISTORY
        // =========================================================

        public async Task<IEnumerable<Payment>> GetPaymentsByPlannerAsync(
            string plannerId)
        {
            var payments = await _paymentRepo.FindAsync(
                p => p.Booking.PlannerId == plannerId,
                "Booking.Quotation.Enquiry.Customer.User");

            return payments.OrderByDescending(p => p.PaymentDate);
        }


        // =========================================================
        // PROCESS SUCCESSFUL PAYMENT
        // =========================================================

        public async Task<bool> ProcessPaymentSuccessAsync(
            int bookingId,
            string transactionId,
            decimal amount,
            string paymentType)
        {
            var bookings = await _bookingRepo.FindAsync(
                b => b.Id == bookingId,
                "Quotation.Enquiry.Customer.User,Quotation.Enquiry.EventPlanner.User,Payments");

            var booking = bookings.FirstOrDefault();

            if (booking == null)
                return false;


            // =========================================================
            // DETERMINE EXPECTED PAYMENT
            // =========================================================

            var successfulPayments = booking.Payments?
                .Where(p => p.Status == "Success")
                .ToList()
                ?? new List<Payment>();

            var paidAmount = successfulPayments.Sum(p => p.Amount);

            decimal expectedAmount;

            if (paymentType == "Advance")
            {
                if (successfulPayments.Any(
                    p => p.PaymentType == "Advance"))
                {
                    return false;
                }

                expectedAmount = booking.AdvanceAmount;
            }
            else if (paymentType == "Remaining")
            {
                if (!successfulPayments.Any(
                    p => p.PaymentType == "Advance"))
                {
                    return false;
                }

                expectedAmount = booking.TotalAmount - paidAmount;

                if (expectedAmount <= 0)
                    return false;
            }
            else
            {
                return false;
            }


            // =========================================================
            // VERIFY PAYMENT AMOUNT
            // =========================================================

            if (amount != expectedAmount)
                return false;


            // =========================================================
            // GET ACTUAL PAYMENT METHOD FROM RAZORPAY
            // =========================================================

            string paymentMethod = "Online";

            try
            {
                var client = new Razorpay.Api.RazorpayClient(
                    _razorpaySettings.KeyId,
                    _razorpaySettings.KeySecret);

                var payment =
                    client.Payment.Fetch(transactionId);

                var razorpayMethod =
                    payment["method"]?.ToString();

                if (!string.IsNullOrWhiteSpace(razorpayMethod))
                {
                    paymentMethod = razorpayMethod.ToLower() switch
                    {
                        "upi" => "UPI",
                        "card" => "Card",
                        "netbanking" => "Net Banking",
                        "wallet" => "Wallet",
                        "emi" => "EMI",
                        "bank_transfer" => "Bank Transfer",
                        _ => "Online"
                    };
                }
            }
            catch
            {
                paymentMethod = "Online";
            }


            // =========================================================
            // CREATE PAYMENT RECORD
            // =========================================================

            var newPayment = new Payment
            {
                BookingId = bookingId,
                TransactionId = transactionId,
                PaymentMethod = paymentMethod,
                Amount = amount,
                Status = "Success",
                PaymentType = paymentType,
                PaymentDate = DateTime.Now
            };

            await _paymentRepo.AddAsync(newPayment);


            // =========================================================
            // UPDATE BOOKING STATUS
            // =========================================================

            var newPaidAmount = paidAmount + amount;

            if (newPaidAmount >= booking.TotalAmount)
            {
                if (booking.Status == "Upcoming")
                {
                    booking.Status = "Confirmed";
                    _bookingRepo.Update(booking);
                }
            }
            else if (paymentType == "Advance")
            {
                if (booking.Status == "Upcoming")
                {
                    booking.Status = "Confirmed";
                    _bookingRepo.Update(booking);
                }
            }

            await _unitOfWork.SaveChangesAsync();


            // =========================================================
            // PAYMENT DATE FOR EMAIL
            // =========================================================

            var paymentDate =
                newPayment.PaymentDate.ToString("dd MMM yyyy, hh:mm tt");


            // =========================================================
            // SEND PAYMENT SUCCESS EMAIL TO CUSTOMER
            // =========================================================

            if (booking.Quotation?.Enquiry?.Customer?.User?.Email != null)
            {
                var customerEmailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Payment Successful</title>
</head>

<body style=""margin:0; padding:0; background:#f4f7f6; font-family:Arial,Helvetica,sans-serif;"">

    <div style=""max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 20px rgba(0,0,0,0.08);"">

        <div style=""background:#12615c; padding:30px; text-align:center; color:#ffffff;"">

            <h1 style=""margin:0; font-size:28px;"">
                EventBridge
            </h1>

            <p style=""margin:8px 0 0; font-size:14px; opacity:0.9;"">
                Event Planning Marketplace
            </p>

        </div>

        <div style=""padding:35px;"">

            <div style=""text-align:center; margin-bottom:25px;"">

                <div style=""font-size:42px; color:#12615c;"">
    <i class=""fa-solid fa-circle-check""></i>
</div>

                <h2 style=""color:#263238; margin:10px 0;"">
                    Payment Successful
                </h2>

                <p style=""color:#607d8b; font-size:15px; line-height:1.6;"">
                    Your payment has been successfully received.
                    Thank you for using EventBridge.
                </p>

            </div>

            <div style=""background:#f4f7f6; border-radius:8px; padding:22px;"">

                <h3 style=""margin-top:0; color:#263238;"">
                    Payment Details
                </h3>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Payment Type:</strong>
                    {paymentType}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Amount Paid:</strong>
                    INR {amount:N2}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Payment Method:</strong>
                    {paymentMethod}
                </p>

                <p style=""margin:10px 0; color:#607d8b; word-break:break-all;"">
                    <strong>Transaction ID:</strong>
                    {transactionId}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Booking ID:</strong>
                    {bookingId}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Payment Date:</strong>
                    {paymentDate}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Total Booking Amount:</strong>
                    INR {booking.TotalAmount:N2}
                </p>

                <p style=""margin:10px 0 0; color:#607d8b;"">
                    <strong>Booking Status:</strong>
                    {booking.Status}
                </p>

            </div>

            <p style=""color:#607d8b; font-size:14px; line-height:1.6; margin-top:25px;"">
                Please keep this email for your records.
                If you have any questions regarding your booking or payment,
                please contact EventBridge support.
            </p>

        </div>

        <div style=""background:#f4f7f6; padding:20px; text-align:center;"">

            <p style=""margin:0; color:#9aa5a1; font-size:12px;"">
                This is an automated email from EventBridge.
            </p>

        </div>

    </div>

</body>
</html>";

                await _emailService.SendEmailAsync(
                    booking.Quotation.Enquiry.Customer.User.Email,
                    "Payment Successful - EventBridge",
                    customerEmailBody);
            }


            // =========================================================
            // SEND PAYMENT RECEIVED EMAIL TO PLANNER
            // =========================================================

            if (booking.Quotation?.Enquiry?.EventPlanner?.User?.Email != null)
            {
                var plannerEmailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Payment Received</title>
</head>

<body style=""margin:0; padding:0; background:#f4f7f6; font-family:Arial,Helvetica,sans-serif;"">

    <div style=""max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 20px rgba(0,0,0,0.08);"">

        <div style=""background:#12615c; padding:30px; text-align:center; color:#ffffff;"">

            <h1 style=""margin:0; font-size:28px;"">
                EventBridge
            </h1>

            <p style=""margin:8px 0 0; font-size:14px; opacity:0.9;"">
                Event Planning Marketplace
            </p>

        </div>

        <div style=""padding:35px;"">

            <div style=""text-align:center; margin-bottom:25px;"">

                <div style=""font-size:42px; color:#12615c;"">
    <i class=""fa-solid fa-circle-check""></i>
</div>

                <h2 style=""color:#263238; margin:10px 0;"">
                    Payment Received
                </h2>

                <p style=""color:#607d8b; font-size:15px; line-height:1.6;"">
                    A payment for one of your EventBridge bookings
                    has been successfully received.
                </p>

            </div>

            <div style=""background:#f4f7f6; border-radius:8px; padding:22px;"">

                <h3 style=""margin-top:0; color:#263238;"">
                    Payment Details
                </h3>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Payment Type:</strong>
                    {paymentType}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Amount Received:</strong>
                    INR {amount:N2}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Payment Method:</strong>
                    {paymentMethod}
                </p>

                <p style=""margin:10px 0; color:#607d8b; word-break:break-all;"">
                    <strong>Transaction ID:</strong>
                    {transactionId}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Booking ID:</strong>
                    {bookingId}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Payment Date:</strong>
                    {paymentDate}
                </p>

                <p style=""margin:10px 0 0; color:#607d8b;"">
                    <strong>Total Booking Amount:</strong>
                    INR {booking.TotalAmount:N2}
                </p>

            </div>

            <p style=""color:#607d8b; font-size:14px; line-height:1.6; margin-top:25px;"">
                The payment has been recorded successfully in EventBridge.
                You can view the payment details from your planner dashboard.
            </p>

        </div>

        <div style=""background:#f4f7f6; padding:20px; text-align:center;"">

            <p style=""margin:0; color:#9aa5a1; font-size:12px;"">
                This is an automated email from EventBridge.
            </p>

        </div>

    </div>

</body>
</html>";

                await _emailService.SendEmailAsync(
                    booking.Quotation.Enquiry.EventPlanner.User.Email,
                    "Payment Received - EventBridge",
                    plannerEmailBody);
            }


            return true;
        }


        // =========================================================
        // PROCESS FAILED PAYMENT
        // =========================================================

        public async Task<bool> ProcessPaymentFailureAsync(
    int bookingId,
    string transactionId,
    decimal amount,
    string paymentType)
        {
            // Prevent the same Razorpay failed payment
            // from being recorded more than once.
            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                var existingPayment = await _paymentRepo.FindAsync(
                    p => p.TransactionId == transactionId &&
                         p.Status == "Failed");

                if (existingPayment.Any())
                {
                    return false;
                }
            }

            string paymentMethod = "Online";

            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                try
                {
                    var client = new Razorpay.Api.RazorpayClient(
                        _razorpaySettings.KeyId,
                        _razorpaySettings.KeySecret);

                    var payment =
                        client.Payment.Fetch(transactionId);

                    var razorpayMethod =
                        payment["method"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(razorpayMethod))
                    {
                        paymentMethod = razorpayMethod.ToLower() switch
                        {
                            "upi" => "UPI",
                            "card" => "Card",
                            "netbanking" => "Net Banking",
                            "wallet" => "Wallet",
                            "emi" => "EMI",
                            "bank_transfer" => "Bank Transfer",
                            _ => "Online"
                        };
                    }
                }
                catch
                {
                    paymentMethod = "Online";
                }
            }

            var newPayment = new Payment
            {
                BookingId = bookingId,
                TransactionId = transactionId,
                PaymentMethod = paymentMethod,
                Amount = amount,
                Status = "Failed",
                PaymentType = paymentType,
                PaymentDate = DateTime.Now
            };

            await _paymentRepo.AddAsync(newPayment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // =========================================================
        // SEND PAYMENT FAILURE EMAIL
        // =========================================================

        public async Task SendPaymentFailureEmailAsync(
            int bookingId,
            decimal amount,
            string paymentType,
            string errorDescription)
        {
            var bookings = await _bookingRepo.FindAsync(
                b => b.Id == bookingId,
                "Quotation.Enquiry.Customer.User,Quotation.Enquiry.EventPlanner.User");

            var booking = bookings.FirstOrDefault();

            if (booking == null)
                return;

            var paymentDate =
                DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");

            var customerEmail =
                booking.Quotation?.Enquiry?.Customer?.User?.Email;

            var plannerEmail =
                booking.Quotation?.Enquiry?.EventPlanner?.User?.Email;


            // =========================================================
            // SAME EMAIL BODY FOR CUSTOMER AND PLANNER
            // =========================================================

            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Payment Failed</title>
</head>

<body style=""margin:0; padding:0; background:#f4f7f6; font-family:Arial,Helvetica,sans-serif;"">

    <div style=""max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 20px rgba(0,0,0,0.08);"">

        <div style=""background:#12615c; padding:30px; text-align:center; color:#ffffff;"">

            <h1 style=""margin:0; font-size:28px;"">
                EventBridge
            </h1>

            <p style=""margin:8px 0 0; font-size:14px; opacity:0.9;"">
                Event Planning Marketplace
            </p>

        </div>

        <div style=""padding:35px;"">

            <div style=""text-align:center; margin-bottom:25px;"">

                <div style=""font-size:42px; color:#c62828;"">
                    <i class=""fa-solid fa-circle-xmark""></i>
                </div>

                <h2 style=""color:#263238; margin:10px 0;"">
                    Payment Failed
                </h2>

                <p style=""color:#607d8b; font-size:15px; line-height:1.6;"">
                    The payment attempt for your EventBridge booking
                    was not successful.
                </p>

            </div>

            <div style=""background:#f4f7f6; border-radius:8px; padding:22px;"">

                <h3 style=""margin-top:0; color:#263238;"">
                    Payment Details
                </h3>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Payment Type:</strong>
                    {paymentType}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Amount:</strong>
                    INR {amount:N2}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Booking ID:</strong>
                    {bookingId}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Date:</strong>
                    {paymentDate}
                </p>

                <p style=""margin:10px 0; color:#607d8b;"">
                    <strong>Reason:</strong>
                    {errorDescription}
                </p>

            </div>

            <p style=""color:#607d8b; font-size:14px; line-height:1.6; margin-top:25px;"">
                No payment has been recorded for this attempt.
                You may try the payment again from your EventBridge booking.
            </p>

        </div>

        <div style=""background:#f4f7f6; padding:20px; text-align:center;"">

            <p style=""margin:0; color:#9aa5a1; font-size:12px;"">
                This is an automated email from EventBridge.
            </p>

        </div>

    </div>

</body>
</html>";


            // =========================================================
            // SEND TO CUSTOMER
            // =========================================================

            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                await _emailService.SendEmailAsync(
                    customerEmail,
                    "Payment Failed - EventBridge",
                    emailBody);
            }


            // =========================================================
            // SEND TO PLANNER
            // =========================================================

            if (!string.IsNullOrWhiteSpace(plannerEmail))
            {
                await _emailService.SendEmailAsync(
                    plannerEmail,
                    "Payment Failed - EventBridge",
                    emailBody);
            }
        }
    }
}