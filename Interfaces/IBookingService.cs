using EventBridge.Models;

namespace EventBridge.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<IEnumerable<Booking>> GetBookingsByCustomerAsync(string customerId);
        Task<IEnumerable<Booking>> GetBookingsByPlannerAsync(string plannerId);
        Task<bool> UpdateBookingStatusAsync(int bookingId, string status, string plannerId);
    }
}
