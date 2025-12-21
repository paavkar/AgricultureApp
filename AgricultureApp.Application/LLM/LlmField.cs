using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.LLM
{
    public class LlmField
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Size { get; set; }
        public string SizeUnit { get; set; }
        public string Status { get; set; }
        public string SoilType { get; set; }
        public string FarmId { get; set; }
        public string OwnerFarmId { get; set; }

        public List<LlmCultivation> Cultivations { get; set; } = [];
        public FarmDto CurrentFarm { get; set; }
        public FarmDto OwnerFarm { get; set; }

        public static LlmField FromDto(FieldDto fieldDto)
        {
            return new LlmField
            {
                Id = fieldDto.Id,
                Name = fieldDto.Name,
                Size = fieldDto.Size,
                SizeUnit = fieldDto.SizeUnit,
                FarmId = fieldDto.FarmId,
                OwnerFarmId = fieldDto.OwnerFarmId,
                CurrentFarm = fieldDto.CurrentFarm,
                OwnerFarm = fieldDto.OwnerFarm
            };
        }
    }
}
