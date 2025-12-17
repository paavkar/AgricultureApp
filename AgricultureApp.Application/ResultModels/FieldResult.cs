using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.ResultModels
{
    public class FieldResult : BaseResult
    {
        public FieldDto? Field { get; set; }
        public IEnumerable<FieldDto>? Fields { get; set; }
    }
}
