using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.InvoicePayment;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class InvoicePaymentService : IInvoicePaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountingService _accountingService;

        public InvoicePaymentService(ApplicationDbContext context, IAccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        public async Task<IEnumerable<InvoicePaymentDto>> GetPaymentsByInvoiceIdAsync(Guid invoiceId, Guid tenantId)
        {
            var payments = await _context.InvoicePayments
                .Include(p => p.SupplierInvoice)
                .Where(p => p.SupplierInvoiceId == invoiceId && p.SupplierInvoice!.TenantId == tenantId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        public async Task<InvoicePaymentDto> CreatePaymentAsync(CreateInvoicePaymentDto dto, Guid tenantId, Guid userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Get Invoice
                    var invoice = await _context.SupplierInvoices
                        .FirstOrDefaultAsync(i => i.Id == dto.SupplierInvoiceId && i.TenantId == tenantId);

                    if (invoice == null) throw new KeyNotFoundException("Supplier Invoice not found.");

                    // 2. Create Payment
                    var payment = new InvoicePayment
                    {
                        SupplierInvoiceId = dto.SupplierInvoiceId,
                        PaymentDate = dto.PaymentDate,
                        AmountPaid = dto.AmountPaid,
                        PaymentMethod = dto.PaymentMethod,
                        CreatedBy = userId,
                        SupplierOrCustomer = "SUPPLIER"
                    };

                    _context.InvoicePayments.Add(payment);

                    // 3. Update Invoice Totals
                    invoice.TotalPaid += dto.AmountPaid;

                    // 4. Update Status
                    if (invoice.TotalPaid >= invoice.TotalAmount)
                    {
                        invoice.Status = "Paid";
                    }
                    else if (invoice.TotalPaid > 0)
                    {
                        invoice.Status = "PartiallyPaid";
                    }
                    else
                    {
                        invoice.Status = "Unpaid";
                    }

                    _context.SupplierInvoices.Update(invoice);
                    await _context.SaveChangesAsync();

                    var paymentDto = MapToDto(payment);
                    
                    // 5. Post Accounting
                    await _accountingService.PostInvoicePaymentTransactionAsync(paymentDto, userId);

                    await transaction.CommitAsync();

                    payment.SupplierInvoice = invoice; // For DTO mapping
                    return paymentDto;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static InvoicePaymentDto MapToDto(InvoicePayment p)
        {
            return new InvoicePaymentDto
            {
                Id = p.Id,
                SupplierInvoiceId = p.SupplierInvoiceId,
                InvoiceNumber = p.SupplierInvoice?.InvoiceNumber ?? "Unknown",
                PaymentDate = p.PaymentDate,
                AmountPaid = p.AmountPaid,
                PaymentMethod = p.PaymentMethod,
                // CreatedAt = p.CreatedAt // BaseEntity check
            };
        }
    }
}
