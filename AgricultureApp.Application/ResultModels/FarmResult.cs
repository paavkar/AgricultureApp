using AgricultureApp.Application.DTOs;

namespace AgricultureApp.Application.ResultModels
{
    public class FarmResult<TFarm> : BaseResult
    {
        public TFarm? Farm { get; set; }
        public UpdateFarmDto? UpdatedFarm { get; set; }
        public IEnumerable<TFarm>? Farms { get; set; }
    }
}
