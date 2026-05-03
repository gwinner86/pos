using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities
{
    public class Feature
    {
        [Key]
        public int FeatureId { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<FeatureDetail> FeatureDetails { get; set; } = new List<FeatureDetail>();
    }
}
