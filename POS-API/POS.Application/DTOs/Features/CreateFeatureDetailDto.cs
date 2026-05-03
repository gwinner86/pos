namespace POS.Application.DTOs.Features
{
    public class CreateFeatureDetailDto
    {
        public string SpecificFeature { get; set; } = string.Empty;
        public string SpecificFeatureValue { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }
}
