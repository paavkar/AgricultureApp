using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.LLM
{
    public class LlmCultivation
    {
        public string Id { get; set; }
        public string Crop { get; set; }
        public double? ExpectedYield { get; set; }
        public double? ActualYield { get; set; }
        public string YieldUnit { get; set; }
        public string Status { get; set; }
        public DateTime PlantingDate { get; set; }
        public DateTime? HarvestDate { get; set; }
        public string FieldId { get; set; }
        public string FarmId { get; set; }

        public FarmDto CultivatedFarm { get; set; }

        public static LlmCultivation FromDto(FieldCultivationDto dto)
        {
            return new LlmCultivation
            {
                Id = dto.Id,
                Crop = dto.Crop,
                ExpectedYield = dto.ExpectedYield,
                ActualYield = dto.ActualYield,
                YieldUnit = dto.YieldUnit,
                PlantingDate = dto.PlantingDate,
                HarvestDate = dto.HarvestDate,
                FieldId = dto.FieldId,
                FarmId = dto.CultivatedFarm.Id,
                CultivatedFarm = dto.CultivatedFarm
            };
        }
    }
}
