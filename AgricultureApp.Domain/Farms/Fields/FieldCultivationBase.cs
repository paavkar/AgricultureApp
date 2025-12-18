namespace AgricultureApp.Domain.Farms
{
    public class FieldCultivationBase
    {
        public string Id { get; set; }
        public string Crop { get; set; }
        public double? ExpectedYield { get; set; }
        public double? ActualYield { get; set; }
        public string YieldUnit { get; set; }
        public CultivationStatus Status { get; set; }
        public DateTime PlantingDate { get; set; }
        public DateTime? HarvestDate { get; set; }
        public string FieldId { get; set; }
        public string FarmId { get; set; }
    }
}
