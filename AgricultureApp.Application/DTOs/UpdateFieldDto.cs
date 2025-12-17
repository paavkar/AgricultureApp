using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.DTOs
{
    public class UpdateFieldDto
    {
        public string FieldId { get; set; }
        public string Name { get; set; }
        public double Size { get; set; }
        public string SizeUnit { get; set; }
        public FieldStatus Status { get; set; }
        public SoilType SoilType { get; set; }

        public string OwnerFarmId { get; set; }
    }
}
