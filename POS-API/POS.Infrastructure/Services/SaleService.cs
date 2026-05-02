using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Sale;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountingService _accountingService;

        public SaleService(ApplicationDbContext context, IAccountingService accountingService)
        {
            _context = context;
            _accountingService = accountingService;
        }

        public async Task<IEnumerable<SaleDto>> GetSalesAsync(Guid tenantId, Guid companyId, Guid? customerId = null, Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Location)
                .Include(s => s.Creator)
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .Include(s => s.Details)
                    .ThenInclude(d => d.ProductVariant)
                .Where(s => s.TenantId == tenantId && s.CompanyId == companyId && 
                            (!customerId.HasValue || s.CustomerId == customerId) &&
                            (!userId.HasValue || s.CreatedBy == userId) &&
                            (!startDate.HasValue || s.SaleDate >= startDate.Value) &&
                            (!endDate.HasValue || s.SaleDate <= endDate.Value))
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return sales.Select(MapToDto).ToList();
        }

        public async Task<SaleDto> GetSaleByIdAsync(Guid id, Guid tenantId)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Location)
                .Include(s => s.Creator)
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .Include(s => s.Details)
                    .ThenInclude(d => d.ProductVariant)
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

            if (sale == null) throw new KeyNotFoundException("Sale not found.");

            return MapToDto(sale);
        }

        public async Task<SaleDto> CreateSaleAsync(CreateSaleDto dto, Guid tenantId, Guid userId, Guid companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Validate Location & Customer
                    var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == dto.LocationId && l.TenantId == tenantId);
                    if (location == null) throw new KeyNotFoundException("Location not found.");

                    if (dto.CustomerId.HasValue)
                    {
                        var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId.Value && c.TenantId == tenantId);
                        if (!customerExists) throw new KeyNotFoundException("Customer not found.");
                    }

                    // 2. Fetch Active VATs
                    var activeVats = await _context.SetupVATs
                        .Where(v => v.TenantId == tenantId && v.CompanyId == companyId && v.IsActive)
                        .ToListAsync();

                    // 3. Generate Sale Number (Simple random for now, or fetch next sequence)
                    // In real app, use a sequence table or strict logic.
                    // Format: SOR-{Year}-{Random/Sequence}
                    var saleNumber = $"SOR-{DateTime.UtcNow.Year}-{new Random().Next(10000, 99999)}";

                    // 3. Create Header
                    var sale = new Sale
                    {
                        TenantId = tenantId,
                        CompanyId = companyId,
                        LocationId = dto.LocationId,
                        CustomerId = dto.CustomerId,
                        SaleDate = dto.SaleDate,
                        SaleNumber = saleNumber,
                        Status = "Completed", // Immediate completion for POS
                        PaymentMethod = dto.PaymentMethod,
                        CreatedBy = userId,
                        Details = new List<SaleDetail>()
                    };

                    decimal totalAmount = 0;

                    // 4. Process Details & Deduct Stock
                    foreach (var itemDto in dto.Details)
                    {
                        var variant = await _context.ProductVariants
                            .Include(pv => pv.Product)
                            .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId && pv.Product!.TenantId == tenantId);
                        
                        if (variant == null) throw new KeyNotFoundException($"Product Variant {itemDto.ProductVariantId} not found.");

                        // Determine Inventory Record for this Location & Variant
                        // Note: If multiple batches exist, we might need FIFO? 
                        // Simplified: We assume 'Inventory' table holds quantity per location per variant?
                        // Let's check Inventory entity? 
                        // Wait, previous sessions showed Inventory entity has LocationId and ProductVariantId.
                        var inventory = await _context.Inventory
                            .FirstOrDefaultAsync(i => i.ProductVariantId == variant.Id && i.LocationId == dto.LocationId);

                        // If no inventory record, or insufficient stock?
                        if (inventory == null || inventory.Quantity < itemDto.Quantity)
                        {
                            var locationName = await _context.Locations
                                .Where(l => l.Id == dto.LocationId)
                                .Select(l => l.LocationName)
                                .FirstOrDefaultAsync() ?? "Unknown Location";

                            throw new InvalidOperationException($"Insufficient stock for {variant.VariantSku} (VarID: {variant.Id}) at {locationName} (LocID: {dto.LocationId}). InvFound: {inventory != null}, Qty: {inventory?.Quantity ?? 0}, Req: {itemDto.Quantity}");
                        }

                        // Deduct Stock from Inventory (Location Specific)
                        inventory.Quantity -= itemDto.Quantity;
                        _context.Inventory.Update(inventory);

                        // Global stock values removed from Product/Variant tables
                        // Also trigger 'InventoryAudit' ? (Good practice, but omitting for brevity unless requested. Sale record itself is proof).

                        // Calculate Line
                        var lineTotal = (itemDto.UnitPrice * itemDto.Quantity) - itemDto.Discount;
                        
                        string? vatDetailsJson = null;
                        if (!variant.Product!.IsVatExcluded && activeVats.Any())
                        {
                            var appliedVats = new List<object>();
                            decimal lineVatTotal = 0;

                            if (location.VatCalculationType == 1) // Exclusive
                            {
                                foreach(var v in activeVats)
                                {
                                    var vatAmt = lineTotal * (v.Rate / 100m);
                                    appliedVats.Add(new { v.Name, v.Rate, Amount = vatAmt });
                                    lineVatTotal += vatAmt;
                                }
                                lineTotal += lineVatTotal; // Add VAT to total
                            }
                            else if (location.VatCalculationType == 2) // Inclusive
                            {
                                var totalVatRate = activeVats.Sum(v => v.Rate);
                                var baseLineTotal = lineTotal / (1 + (totalVatRate / 100m));
                                
                                foreach(var v in activeVats)
                                {
                                    var vatAmt = baseLineTotal * (v.Rate / 100m);
                                    appliedVats.Add(new { v.Name, v.Rate, Amount = vatAmt });
                                }
                            }
                            
                            vatDetailsJson = JsonSerializer.Serialize(appliedVats);
                        }

                        totalAmount += lineTotal;

                        sale.Details.Add(new SaleDetail
                        {
                            ProductId = variant.ProductId,
                            ProductVariantId = variant.Id,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice,
                            Discount = itemDto.Discount,
                            LineTotal = lineTotal,
                            VatDetails = vatDetailsJson
                        });
                    }

                    sale.TotalAmount = totalAmount;

                    _context.Sales.Add(sale);
                    await _context.SaveChangesAsync();

                    // 5. Post Accounting Transaction for POS Sale (Direct Cash/Income + Inventory Cost)
                    // "Debit Cash on Hand, Credit Inventory, Credit Income Account"
                    var saleDto = MapToDto(sale);
                    await _accountingService.PostDirectSaleTransactionAsync(saleDto, userId);
                    
                    // Note: We are not creating a separate Payment GL entry because PostDirectSaleTransactionAsync handles the Cash Debit.
                    // We also skip creating a SalePayment entity here for now as per previous logic, 
                    // though in a full system we'd likely want to persist the payment record itself.

                    await transaction.CommitAsync();

                    return await GetSaleByIdAsync(sale.Id, tenantId);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static SaleDto MapToDto(Sale s)
        {
            var dto = new SaleDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                LocationId = s.LocationId,
                LocationName = s.Location?.LocationName ?? "Unknown",
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? $"{s.Customer.FirstName} {s.Customer.LastName}" : "Walk-in Customer",
                SaleNumber = s.SaleNumber,
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount,
                Status = s.Status,
                PaymentMethod = s.PaymentMethod,
                CreatedByUserId = s.CreatedBy,
                CreatedByUserName = s.Creator != null ? $"{s.Creator.FirstName} {s.Creator.LastName}".Trim() : "Unknown",
                CreatedAt = s.CreatedAt
            };

            if (s.Details != null)
            {
                dto.Details = s.Details.Select(d => new SaleDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.ProductName ?? "Unknown",
                    ProductVariantId = d.ProductVariantId,
                    VariantName = d.ProductVariant?.VariantSku ?? "Unknown",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount,
                    LineTotal = d.LineTotal,
                    VatDetails = d.VatDetails
                }).ToList();
            }

            return dto;
        }
    }
}
