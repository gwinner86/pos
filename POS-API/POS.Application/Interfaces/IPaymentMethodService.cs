using POS.Application.DTOs.PaymentMethod;

namespace POS.Application.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<PaymentMethodDto> CreatePaymentMethodAsync(CreatePaymentMethodDto dto, Guid userId);
        Task<PaymentMethodDto> UpdatePaymentMethodAsync(UpdatePaymentMethodDto dto, Guid userId);
        Task<List<PaymentMethodDto>> GetAllPaymentMethodsForUserAsync(Guid userId);
        Task<PaymentMethodDto> GetPaymentMethodByIdAsync(int id);
        Task<bool> DeletePaymentMethodAsync(int id);
    }
}
