using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.DTOs
{
    public class CreateFieldDto
    {
        public string Name { get; set; }
        public double Size { get; set; }
        public string SizeUnit { get; set; }
        public FieldStatus Status { get; set; }
        public SoilType SoilType { get; set; }
        public string FarmId { get; set; }
        public string OwnerFarmId { get; set; }

        public Field ToFieldModel()
        {
            return new Field
            {
                Id = Guid.CreateVersion7().ToString(),
                Name = this.Name,
                Size = this.Size,
                SizeUnit = this.SizeUnit,
                Status = this.Status,
                SoilType = this.SoilType,
                FarmId = this.FarmId,
                OwnerFarmId = this.OwnerFarmId,
                CurrentFarm = null,
                OwnerFarm = null,
            };
        }
    }
}
