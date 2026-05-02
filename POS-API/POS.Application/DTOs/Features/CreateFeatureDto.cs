using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Features
{
    public class CreateFeatureDto
    {
        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateFeatureDetailDto> Details { get; set; } = new List<CreateFeatureDetailDto>();
    }
}
