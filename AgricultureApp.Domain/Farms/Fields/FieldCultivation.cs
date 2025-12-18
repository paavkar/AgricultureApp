namespace AgricultureApp.Domain.Farms
{
    public class FieldCultivation : FieldCultivationBase
    {
        // Navigation Properties
        public Field Field { get; set; }
        public Farm CultivatedFarm { get; set; }

        public FieldCultivationDto ToDto()
        {
            return new FieldCultivationDto
            {
                Id = this.Id,
                Crop = this.Crop,
                ExpectedYield = this.ExpectedYield,
                ActualYield = this.ActualYield,
                YieldUnit = this.YieldUnit,
                Status = this.Status,
                PlantingDate = this.PlantingDate,
                HarvestDate = this.HarvestDate,
                FieldId = this.FieldId,
                FarmId = this.FarmId,
                Field = null,
                CultivatedFarm = null
            };
        }
    }

    public enum CultivationStatus
    {
        Planned = 0,
        Planted,
        Growing,
        Harvested,
        Failed
    }
}
