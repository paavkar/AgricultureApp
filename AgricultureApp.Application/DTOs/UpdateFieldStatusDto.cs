using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.DTOs
{
    public class UpdateFieldStatusDto
    {
        public string FieldId { get; set; }
        public string FarmId { get; set; }
        public FieldStatus Status { get; set; }
    }
}
