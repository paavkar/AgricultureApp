using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.DTOs
{
    public class UpdateFieldCultivationStatusDto
    {
        public string FieldId { get; set; }
        public string FarmId { get; set; }
        public string FieldCultivationId { get; set; }
        public CultivationStatus Status { get; set; }
    }
}
