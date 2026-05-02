using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.SalePayment;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class SalePaymentService : ISalePaymentService
    {
        private readonly ApplicationDbContext _context;

        public SalePaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalePaymentDto> ProcessPaymentAsync(CreateSalePaymentDto dto, Guid userId)
        {
            // 1. Validate Sale
            var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == dto.SaleId);
            if (sale == null)
                throw new KeyNotFoundException($"Sale with ID {dto.SaleId} not found.");

            // 2. Validate Payment Method
            var paymentMethod = await _context.PaymentMethods.FindAsync(dto.PaymentMethodId);
            if (paymentMethod == null || !paymentMethod.IsActive)
                throw new KeyNotFoundException($"Payment Method ID {dto.PaymentMethodId} not found or inactive.");

            // 3. Create Payment Record
            var payment = new SalePayment
            {
                SaleId = dto.SaleId,
                PaymentMethodId = dto.PaymentMethodId,
                Amount = dto.Amount,
                ReferenceNumber = dto.ReferenceNumber,
                PaymentDate = dto.PaymentDate,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.SalePayments.Add(payment);

            // 4. Update Sale status if necessary (Simple logic: if total payments >= total amount, mark completed/paid)
            // Need to sum existing payments
            var existingPayments = await _context.SalePayments
                .Where(p => p.SaleId == dto.SaleId)
                .SumAsync(p => p.Amount);

            var totalPaid = existingPayments + dto.Amount;
            
            // logic can be expanded here

            await _context.SaveChangesAsync();

            return new SalePaymentDto
            {
                Id = payment.Id,
                SaleId = payment.SaleId,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodName = paymentMethod.MethodName,
                Amount = payment.Amount,
                ReferenceNumber = payment.ReferenceNumber,
                PaymentDate = payment.PaymentDate,
                CreatedBy = payment.CreatedBy
            };
        }

        public async Task<List<SalePaymentDto>> GetPaymentsBySaleIdAsync(Guid saleId)
        {
            var payments = await _context.SalePayments
                .Include(p => p.PaymentMethod)
                .Where(p => p.SaleId == saleId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(p => new SalePaymentDto
            {
                Id = p.Id,
                SaleId = p.SaleId,
                PaymentMethodId = p.PaymentMethodId,
                PaymentMethodName = p.PaymentMethod?.MethodName ?? "Unknown",
                Amount = p.Amount,
                ReferenceNumber = p.ReferenceNumber,
                PaymentDate = p.PaymentDate,
                CreatedBy = p.CreatedBy
            }).ToList();
        }
    }
}
