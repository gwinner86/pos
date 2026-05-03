using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Features
{
    public class UpdateFeatureDto
    {
        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
