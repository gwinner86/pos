using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.SupplierInvoice;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class SupplierInvoiceService : ISupplierInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountingService _accountingService;

        public SupplierInvoiceService(ApplicationDbContext context, IAccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        public async Task<IEnumerable<SupplierInvoiceDto>> GetInvoicesAsync(Guid tenantId, Guid companyId)
        {
            var invoices = await _context.SupplierInvoices
                .Include(i => i.Supplier)
                .Where(i => i.TenantId == tenantId && i.CompanyId == companyId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return invoices.Select(MapToDto).ToList();
        }

        public async Task<SupplierInvoiceDto> GetInvoiceByIdAsync(Guid id, Guid tenantId)
        {
            var invoice = await _context.SupplierInvoices
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId);

            if (invoice == null) throw new KeyNotFoundException("Invoice not found.");

            return MapToDto(invoice);
        }

        public async Task<SupplierInvoiceDto> CreateInvoiceAsync(CreateSupplierInvoiceDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == dto.SupplierId && s.TenantId == tenantId);

            if (supplier == null) throw new KeyNotFoundException("Supplier not found.");

            // Validate GoodsReceiptId if provided
            if (dto.GoodsReceiptId.HasValue)
            {
                var goodsReceipt = await _context.GoodsReceipts
                    .FirstOrDefaultAsync(gr => gr.Id == dto.GoodsReceiptId.Value && gr.TenantId == tenantId);
                
                if (goodsReceipt == null)
                {
                    throw new KeyNotFoundException("Goods Receipt not found.");
                }

                // Mark the GoodsReceipt as invoiced
                goodsReceipt.IsInvoiced = true;
                _context.GoodsReceipts.Update(goodsReceipt);
            }

            var invoice = new SupplierInvoice
            {
                TenantId = tenantId,
                CompanyId = companyId,
                SupplierId = dto.SupplierId,
                GoodsReceiptId = dto.GoodsReceiptId,
                InvoiceNumber = dto.InvoiceNumber,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                TotalAmount = dto.TotalAmount,
                TotalPaid = 0,
                Status = "Unpaid",
                CreatedBy = userId
            };

            _context.SupplierInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Reload to get Supplier included if needed, or just map manually
            invoice.Supplier = supplier;
            var invoiceDto = MapToDto(invoice);

            // Post Accounting Entry
            await _accountingService.PostSupplierInvoiceTransactionAsync(invoiceDto, userId);

            return invoiceDto;
        }

        public async Task<SupplierInvoiceDto> UpdateInvoiceAsync(Guid id, UpdateSupplierInvoiceDto dto, Guid tenantId, Guid userId)
        {
            var invoice = await _context.SupplierInvoices
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId);

            if (invoice == null) throw new KeyNotFoundException("Invoice not found.");

            if (!string.IsNullOrEmpty(dto.InvoiceNumber)) invoice.InvoiceNumber = dto.InvoiceNumber;
            if (dto.InvoiceDate.HasValue) invoice.InvoiceDate = dto.InvoiceDate.Value;
            if (dto.DueDate.HasValue) invoice.DueDate = dto.DueDate.Value;
            if (dto.TotalAmount.HasValue) invoice.TotalAmount = dto.TotalAmount.Value;
            if (!string.IsNullOrEmpty(dto.Status)) invoice.Status = dto.Status;
            
            // Recalculate status derived from payments could be logic here, but for now manual status override allowed? 
            // Or usually status is derived. Let's allow manual for now as per DTO.
            
            _context.SupplierInvoices.Update(invoice);
            await _context.SaveChangesAsync();

            return MapToDto(invoice);
        }

        private static SupplierInvoiceDto MapToDto(SupplierInvoice i)
        {
            return new SupplierInvoiceDto
            {
                Id = i.Id,
                SupplierId = i.SupplierId,
                SupplierName = i.Supplier?.SupplierName ?? "Unknown",
                GoodsReceiptId = i.GoodsReceiptId,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                TotalPaid = i.TotalPaid,
                Status = i.Status,
                // CreatedAt = i.CreatedAt // BaseEntity check
            };
        }
    }
}
