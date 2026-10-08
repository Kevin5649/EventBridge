using EventBridge.Models;
using EventBridge.ViewModels;

namespace EventBridge.Interfaces
{
    public interface IQuotationService
    {
        Task<Quotation?> GetQuotationByIdAsync(int id);
        Task<Quotation?> GetQuotationByEnquiryAsync(int enquiryId);
        Task<bool> CreateQuotationAsync(QuotationCreateViewModel model);
        Task<bool> AcceptQuotationAsync(int quotationId, string customerId);
        Task<bool> RejectQuotationAsync(int quotationId, string customerId);
    }
}
