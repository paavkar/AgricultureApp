namespace AgricultureApp.Domain.Farms
{
    public class FieldDto : FieldBase
    {
        public IEnumerable<FieldCultivationDto> Cultivations { get; set; } = [];
        public FarmDto CurrentFarm { get; set; }
        public FarmDto OwnerFarm { get; set; }
    }
}
