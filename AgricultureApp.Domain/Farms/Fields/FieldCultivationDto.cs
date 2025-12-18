namespace AgricultureApp.Domain.Farms
{
    public class FieldCultivationDto : FieldCultivationBase
    {
        public FieldDto Field { get; set; }
        public FarmDto CultivatedFarm { get; set; }
    }
}
