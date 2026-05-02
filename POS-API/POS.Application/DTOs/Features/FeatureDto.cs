using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Features
{
    public class FeatureDto
    {
        public int FeatureId { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<FeatureDetailDto> Details { get; set; } = new List<FeatureDetailDto>();
    }
}
