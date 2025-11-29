using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.ResultModels
{
    public class FarmListResult
    {
        public IEnumerable<Farm>? Farms { get; set; }
    }
}
