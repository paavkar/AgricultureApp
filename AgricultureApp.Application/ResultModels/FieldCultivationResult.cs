using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.ResultModels
{
    public class FieldCultivationResult : BaseResult
    {
        public FieldCultivationDto? FieldCultivation { get; set; }
        public IEnumerable<FieldCultivationDto>? FieldCultivations { get; set; }
    }
}
