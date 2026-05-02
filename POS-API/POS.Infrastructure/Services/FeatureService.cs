using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Features;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Infrastructure.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly ApplicationDbContext _context;

        public FeatureService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<FeatureDto>> GetAllFeaturesAsync()
        {
            var features = await _context.Features
                .Include(f => f.FeatureDetails)
                .AsNoTracking()
                .ToListAsync();

            return features.Select(MapToDto).ToList();
        }

        public async Task<FeatureDto?> GetFeatureByIdAsync(int id)
        {
            var feature = await _context.Features
                .Include(f => f.FeatureDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FeatureId == id);

            if (feature == null) return null;

            return MapToDto(feature);
        }

        public async Task<FeatureDto> CreateFeatureAsync(CreateFeatureDto createFeatureDto)
        {
            var feature = new Feature
            {
                FeatureName = createFeatureDto.FeatureName,
                Description = createFeatureDto.Description,
                IsActive = createFeatureDto.IsActive
            };

            _context.Features.Add(feature);
            await _context.SaveChangesAsync(); 

            if (createFeatureDto.Details != null && createFeatureDto.Details.Any())
            {
                var details = createFeatureDto.Details.Select(d => new FeatureDetail
                {
                    FeatureId = feature.FeatureId,
                    SpecificFeature = d.SpecificFeature,
                    SpecificFeatureValue = d.SpecificFeatureValue,
                    IsEnabled = d.IsEnabled
                }).ToList();

                _context.FeatureDetails.AddRange(details);
                await _context.SaveChangesAsync();
                
                feature.FeatureDetails = details; // Attach for response mapping
            }

            return MapToDto(feature);
        }

        public async Task UpdateFeatureAsync(int id, UpdateFeatureDto updateFeatureDto)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature == null)
            {
                throw new KeyNotFoundException($"Feature with ID {id} not found.");
            }

            feature.FeatureName = updateFeatureDto.FeatureName;
            feature.Description = updateFeatureDto.Description;
            feature.IsActive = updateFeatureDto.IsActive;

            _context.Features.Update(feature);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFeatureAsync(int id)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature != null)
            {
                _context.Features.Remove(feature);
                await _context.SaveChangesAsync();
            }
        }

        private static FeatureDto MapToDto(Feature feature)
        {
            return new FeatureDto
            {
                FeatureId = feature.FeatureId,
                FeatureName = feature.FeatureName,
                Description = feature.Description,
                IsActive = feature.IsActive,
                Details = feature.FeatureDetails?.Select(d => new FeatureDetailDto
                {
                    FeatureDetailId = d.FeatureDetailId,
                    SpecificFeature = d.SpecificFeature,
                    SpecificFeatureValue = d.SpecificFeatureValue,
                    IsEnabled = d.IsEnabled
                }).ToList() ?? new List<FeatureDetailDto>()
            };
        }
    }
}
