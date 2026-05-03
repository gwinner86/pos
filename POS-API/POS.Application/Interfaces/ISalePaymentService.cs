using POS.Application.DTOs.SalePayment;

namespace POS.Application.Interfaces
{
    public interface ISalePaymentService
    {
        Task<SalePaymentDto> ProcessPaymentAsync(CreateSalePaymentDto dto, Guid userId);
        Task<List<SalePaymentDto>> GetPaymentsBySaleIdAsync(Guid saleId);
    }
}
