using EventBridge.Models;

namespace EventBridge.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment?> GetPaymentByIdAsync(int id);

        Task<bool> ProcessPaymentSuccessAsync(
            int bookingId,
            string transactionId,
            decimal amount,
            string paymentType);

        Task<bool> ProcessPaymentFailureAsync(
            int bookingId,
            string transactionId,
            decimal amount,
            string paymentType);

        string CreateRazorpayOrder(
            decimal amount,
            string receiptId);

        bool VerifyRazorpaySignature(
            string orderId,
            string paymentId,
            string signature);

        Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(
            string customerId);

        Task<IEnumerable<Payment>> GetPaymentsByPlannerAsync(
            string plannerId);

        Task SendPaymentFailureEmailAsync(
            int bookingId,
            decimal amount,
            string paymentType,
            string errorDescription);
    }
}