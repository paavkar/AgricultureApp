using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.ResultModels
{
    public class ManagerResult : BaseResult
    {
        public FarmManagerDto? FarmManager { get; set; }
    }
}
