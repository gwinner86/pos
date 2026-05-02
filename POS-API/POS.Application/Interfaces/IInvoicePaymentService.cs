using POS.Application.DTOs.InvoicePayment;

namespace POS.Application.Interfaces
{
    public interface IInvoicePaymentService
    {
        Task<IEnumerable<InvoicePaymentDto>> GetPaymentsByInvoiceIdAsync(Guid invoiceId, Guid tenantId);
        Task<InvoicePaymentDto> CreatePaymentAsync(CreateInvoicePaymentDto dto, Guid tenantId, Guid userId);
    }
}
