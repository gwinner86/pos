using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Settings;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class SetupVATService : ISetupVATService
    {
        private readonly ApplicationDbContext _context;

        public SetupVATService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SetupVATDto>> GetAllAsync(Guid tenantId, Guid companyId)
        {
            return await _context.SetupVATs
                .AsNoTracking()
                .Where(v => v.TenantId == tenantId && v.CompanyId == companyId)
                .Select(v => new SetupVATDto
                {
                    Id = v.Id,
                    TenantId = v.TenantId,
                    CompanyId = v.CompanyId,
                    LocationId = v.LocationId,
                    Name = v.Name,
                    Rate = v.Rate,
                    Description = v.Description,
                    CountryCode = v.CountryCode,
                    MakerId = v.MakerId,
                    MakeDate = v.MakeDate,
                    IsActive = v.IsActive,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<SetupVATDto> GetByIdAsync(Guid id, Guid tenantId)
        {
            var vat = await _context.SetupVATs
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);

            if (vat == null) throw new KeyNotFoundException("VAT Setup not found.");

            return new SetupVATDto
            {
                Id = vat.Id,
                TenantId = vat.TenantId,
                CompanyId = vat.CompanyId,
                LocationId = vat.LocationId,
                Name = vat.Name,
                Rate = vat.Rate,
                Description = vat.Description,
                CountryCode = vat.CountryCode,
                MakerId = vat.MakerId,
                MakeDate = vat.MakeDate,
                IsActive = vat.IsActive,
                CreatedAt = vat.CreatedAt,
                UpdatedAt = vat.UpdatedAt
            };
        }

        public async Task<SetupVATDto> CreateAsync(CreateSetupVATDto dto, Guid tenantId, Guid userId)
        {
            var vat = new SetupVAT
            {
                TenantId = tenantId,
                CompanyId = dto.CompanyId,
                LocationId = dto.LocationId,
                Name = dto.Name,
                Rate = dto.Rate,
                Description = dto.Description,
                CountryCode = dto.CountryCode,
                MakerId = dto.MakerId ?? userId,
                MakeDate = dto.MakeDate ?? DateTime.UtcNow,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.SetupVATs.Add(vat);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(vat.Id, tenantId);
        }

        public async Task<SetupVATDto> UpdateAsync(Guid id, UpdateSetupVATDto dto, Guid tenantId, Guid userId)
        {
            var vat = await _context.SetupVATs.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);
            if (vat == null) throw new KeyNotFoundException("VAT Setup not found.");

            vat.CompanyId = dto.CompanyId;
            vat.LocationId = dto.LocationId;
            vat.Name = dto.Name;
            vat.Rate = dto.Rate;
            vat.Description = dto.Description;
            vat.CountryCode = dto.CountryCode;
            vat.IsActive = dto.IsActive;
            vat.UpdatedAt = DateTime.UtcNow;

            _context.SetupVATs.Update(vat);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(vat.Id, tenantId);
        }

        public async Task DeleteAsync(Guid id, Guid tenantId)
        {
            var vat = await _context.SetupVATs.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);
            if (vat == null) throw new KeyNotFoundException("VAT Setup not found.");

            _context.SetupVATs.Remove(vat);
            await _context.SaveChangesAsync();
        }
    }
}
