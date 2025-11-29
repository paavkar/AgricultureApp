using AgricultureApp.Application.DTOs;
using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.ResultModels
{
    public class FarmResult : BaseResult
    {
        public Farm? Farm { get; set; }
        public UpdateFarmDto? UpdatedFarm { get; set; }
    }
}
