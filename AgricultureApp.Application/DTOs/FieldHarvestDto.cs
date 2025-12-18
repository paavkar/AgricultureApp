using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.DTOs
{
    public class FieldHarvestDto
    {
        public string FieldId { get; set; }
        public string FarmId { get; set; }
        public string FieldCultivationId { get; set; }
        public double ActualYield { get; set; }
        public string YieldUnit { get; set; }
        public CultivationStatus Status { get; set; }
        public DateTime HarvestDate { get; set; }
    }
}
