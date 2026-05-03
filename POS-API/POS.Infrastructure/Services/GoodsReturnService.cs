using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.GoodsReturn;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class GoodsReturnService : IGoodsReturnService
    {
        private readonly ApplicationDbContext _context;

        public GoodsReturnService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GoodsReturnDto> CreateReturnAsync(CreateGoodsReturnDto dto, Guid userId)
        {
            var companyId = await GetCompanyIdForUser(userId);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Create Header
                    // Note: If SalesOrPurchaseId is provided, we could validate it exists, but allowing generic returns for now.
                    
                    var returnHeader = new GoodsReturn
                    {
                        CompanyId = companyId,
                        LocationId = dto.LocationId,
                        UserId = userId,
                        SalesOrPurchaseId = dto.SalesOrPurchaseId,
                        ReturnType = dto.ReturnType,
                        ReturnDate = dto.ReturnDate,
                        ReturnReason = dto.ReturnReason,
                        CreatedBy = userId,
                        Details = new List<GoodsReturnedDetail>()
                    };

                    decimal totalRefund = 0;

                    // 2. Process Details & Update Inventory
                    foreach (var itemDto in dto.Details)
                    {
                        var variant = await _context.ProductVariants
                            .Include(pv => pv.Product)
                            .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId);

                        if (variant == null) throw new KeyNotFoundException($"Variant {itemDto.ProductVariantId} not found.");

                        // Restore Stock if it's a "Refund" (assuming item is put back on shelf)
                        // If "Damaged", maybe we don't restore? 
                        // For simplicity, assuming "Refund" implies restockable.
                        
                        // Find inventory record
                        var inventory = await _context.Inventory
                            .FirstOrDefaultAsync(i => i.ProductVariantId == variant.Id && i.LocationId == dto.LocationId);

                        if (inventory != null)
                        {
                            inventory.Quantity += itemDto.QuantityReturned;
                            _context.Inventory.Update(inventory);
                        }
                        else
                        {
                            // If no inventory record exists (rare if sold from here), create one?
                            // Or maybe just skip if we can't track it. Creating seems safer.
                             var newInventory = new Inventory
                            {
                                LocationId = dto.LocationId,
                                ProductVariantId = variant.Id,
                                Quantity = itemDto.QuantityReturned,
                                ReorderLevel = 0, // Default
                                CreatedBy = userId
                            };
                            _context.Inventory.Add(newInventory);
                        }

                        totalRefund += itemDto.RefundAmount;

                        returnHeader.Details.Add(new GoodsReturnedDetail
                        {
                            ProductVariantId = variant.Id,
                            ProductId = variant.ProductId,
                            SaleItemId = itemDto.SaleItemId,
                            QuantityReturned = itemDto.QuantityReturned,
                            RefundAmount = itemDto.RefundAmount,
                            ReturnOutcome = itemDto.ReturnOutcome,
                            CreatedBy = userId
                        });
                    }

                    returnHeader.TotalRefundAmount = totalRefund;

                    _context.GoodsReturns.Add(returnHeader);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetReturnByIdAsync(returnHeader.Id);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        
        private async Task<Guid> GetCompanyIdForUser(Guid userId)
        {
            var assignment = await _context.UserCompanyAssignments
                .FirstOrDefaultAsync(uca => uca.UserId == userId);
            
            if (assignment == null)
                throw new UnauthorizedAccessException("User is not assigned to any company.");
                
            return assignment.CompanyId;
        }

        public async Task<GoodsReturnDto> GetReturnByIdAsync(Guid id)
        {
            var ret = await _context.GoodsReturns
                .Include(r => r.Location)
                .Include(r => r.Details)
                    .ThenInclude(d => d.ProductVariant)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (ret == null) throw new KeyNotFoundException("Return not found.");

            return MapToDto(ret);
        }

        public async Task<IEnumerable<GoodsReturnDto>> GetReturnsByLocationAsync(Guid locationId)
        {
            var returns = await _context.GoodsReturns
                .Include(r => r.Location)
                .Where(r => r.LocationId == locationId)
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();

            return returns.Select(MapToDto).ToList();
        }

        private static GoodsReturnDto MapToDto(GoodsReturn r)
        {
            return new GoodsReturnDto
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                LocationId = r.LocationId,
                LocationName = r.Location?.LocationName ?? "Unknown",
                SalesOrPurchaseId = r.SalesOrPurchaseId,
                ReturnType = r.ReturnType,
                ReturnDate = r.ReturnDate,
                TotalRefundAmount = r.TotalRefundAmount,
                ReturnReason = r.ReturnReason,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                Details = r.Details.Select(d => new GoodsReturnedDetailDto
                {
                    Id = d.Id,
                    ProductVariantId = d.ProductVariantId,
                    VariantName = d.ProductVariant?.VariantSku ?? "Unknown",
                    SaleItemId = d.SaleItemId,
                    QuantityReturned = d.QuantityReturned,
                    RefundAmount = d.RefundAmount,
                    ReturnOutcome = d.ReturnOutcome
                }).ToList()
            };
        }
    }
}
