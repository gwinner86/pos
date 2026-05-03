using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Location;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;

        public LocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LocationResponse>> GetLocationsAsync(Guid tenantId)
        {
            return await _context.Locations
                .Where(l => l.TenantId == tenantId)
                .Include(l => l.Currency)
                .Select(l => new LocationResponse
                {
                    Id = l.Id,
                    LocationName = l.LocationName,
                    LocationType = l.LocationType,
                    AddressLine1 = l.AddressLine1,
                    CompanyId = l.CompanyId,
                    TenantId = l.TenantId,
                    FeatureId = l.FeatureId,
                    CurrencyId = l.CurrencyId,
                    CurrencySymbol = l.Currency != null ? l.Currency.CurrencySymbol : null,
                    CurrencyCode = l.Currency != null ? l.Currency.CurrencyCode : null,
                    VatCalculationType = l.VatCalculationType
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LocationResponse>> GetLocationsByCompanyIdAsync(Guid companyId, Guid tenantId)
        {
            return await _context.Locations
                .Where(l => l.CompanyId == companyId && l.TenantId == tenantId)
                .Include(l => l.Currency)
                .Select(l => new LocationResponse
                {
                    Id = l.Id,
                    LocationName = l.LocationName,
                    LocationType = l.LocationType,
                    AddressLine1 = l.AddressLine1,
                    CompanyId = l.CompanyId,
                    TenantId = l.TenantId,
                    FeatureId = l.FeatureId,
                    CurrencyId = l.CurrencyId,
                    CurrencySymbol = l.Currency != null ? l.Currency.CurrencySymbol : null,
                    CurrencyCode = l.Currency != null ? l.Currency.CurrencyCode : null,
                    VatCalculationType = l.VatCalculationType
                })
                .ToListAsync();
        }

        public async Task<LocationResponse?> GetLocationByIdAsync(Guid locationId, Guid tenantId)
        {
            var location = await _context.Locations
                .Include(l => l.Currency)
                .FirstOrDefaultAsync(l => l.Id == locationId && l.TenantId == tenantId);

            if (location == null) return null;

            return new LocationResponse
            {
                Id = location.Id,
                LocationName = location.LocationName,
                LocationType = location.LocationType,
                AddressLine1 = location.AddressLine1,
                CompanyId = location.CompanyId,
                TenantId = location.TenantId,
                FeatureId = location.FeatureId,
                CurrencyId = location.CurrencyId,
                CurrencySymbol = location?.Currency?.CurrencySymbol,
                CurrencyCode = location?.Currency?.CurrencyCode,
                VatCalculationType = location.VatCalculationType
            };
        }

        public async Task<Guid> CreateLocationAsync(CreateLocationDto request, Guid tenantId, Guid companyId)
        {
            // Validate Company exists and belongs to Tenant
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId && c.TenantId == tenantId);
            if (!companyExists)
            {
                throw new KeyNotFoundException("Company not found or does not belong to this tenant.");
            }

            var location = new Location
            {
                LocationName = request.LocationName,
                LocationType = request.LocationType,
                AddressLine1 = request.AddressLine1,
                CompanyId = companyId,
                TenantId = tenantId,
                FeatureId = request.FeatureId,
                CurrencyId = request.CurrencyId
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return location.Id;
        }

        public async Task<bool> UpdateLocationAsync(Guid locationId, UpdateLocationDto request, Guid tenantId)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == locationId && l.TenantId == tenantId);
            if (location == null) return false;

            location.LocationName = request.LocationName;
            location.LocationType = request.LocationType;
            location.AddressLine1 = request.AddressLine1;
            location.CurrencyId = request.CurrencyId;
            // CompanyId usually shouldn't change easily, or need specific validation if allowed. Keeping it fixed for now.

            _context.Locations.Update(location);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLocationAsync(Guid locationId, Guid tenantId)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == locationId && l.TenantId == tenantId);
            if (location == null) return false;

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
