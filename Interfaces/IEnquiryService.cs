using EventBridge.Models;
using EventBridge.ViewModels;

namespace EventBridge.Interfaces
{
    public interface IEnquiryService
    {
        Task<Enquiry?> GetEnquiryByIdAsync(int id);
        Task<IEnumerable<Enquiry>> GetEnquiriesByCustomerAsync(string customerId);
        Task<IEnumerable<Enquiry>> GetEnquiriesByPlannerAsync(string plannerId);
        Task<bool> CreateEnquiryAsync(string customerId, EnquiryCreateViewModel model);
        Task<bool> UpdateEnquiryStatusAsync(int enquiryId, string status);
    }
}
