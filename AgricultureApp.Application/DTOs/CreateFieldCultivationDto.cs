using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.DTOs
{
    public class CreateFieldCultivationDto
    {
        public string Crop { get; set; }
        public double? ExpectedYield { get; set; }
        public string YieldUnit { get; set; }
        public CultivationStatus Status { get; set; }
        public DateTime PlantingDate { get; set; }
        public string FieldId { get; set; }
        public string FarmId { get; set; }

        public FieldCultivation ToFieldCultivationModel()
        {
            return new FieldCultivation
            {
                Id = Guid.CreateVersion7().ToString(),
                Crop = this.Crop,
                ExpectedYield = this.ExpectedYield,
                YieldUnit = this.YieldUnit,
                Status = this.Status,
                PlantingDate = this.PlantingDate,
                FieldId = this.FieldId,
                FarmId = this.FarmId,
            };
        }
    }
}
