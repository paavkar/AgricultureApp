namespace AgricultureApp.Domain.Farms
{
    public class Field : FieldBase
    {
        // Navigation Properties
        public IEnumerable<FieldCultivation> Cultivations { get; set; } = [];
        public Farm CurrentFarm { get; set; }
        public Farm OwnerFarm { get; set; }

        public FieldDto ToDto()
        {
            return new FieldDto
            {
                Id = this.Id,
                Name = this.Name,
                Size = this.Size,
                SizeUnit = this.SizeUnit,
                Status = this.Status,
                SoilType = this.SoilType,
                FarmId = this.FarmId,
                OwnerFarmId = this.OwnerFarmId,
                CurrentFarm = null,
                OwnerFarm = null
            };
        }
    }

    public enum FieldStatus
    {
        Active = 0,
        Inactive,
        UnderMaintenance
    }

    public enum SoilType
    {
        Sandy = 0,
        Clay,
        Silty,
        Peaty,
        Chalky,
        Loamy,
        Other,
        Generic
    }
}
