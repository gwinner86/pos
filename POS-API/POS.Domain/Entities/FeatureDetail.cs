using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities
{
    public class FeatureDetail
    {
        [Key]
        public int FeatureDetailId { get; set; }
        public int FeatureId { get; set; }
        // TenantId removed as per user request (Global Details)
        public string SpecificFeature { get; set; } = string.Empty;
        public string SpecificFeatureValue { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = false;

        public Feature? Feature { get; set; }
    }
}
