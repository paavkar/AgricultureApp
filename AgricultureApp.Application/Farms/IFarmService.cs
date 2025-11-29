using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.ResultModels;

namespace AgricultureApp.Application.Farms
{
    public interface IFarmService
    {
        Task<FarmResult> CreateAsync(CreateFarmDto farmDto, string userId);
        Task<FarmResult> GetByIdAsync(string farmId);
        Task<FarmListResult> GetByOwnerAsync(string ownerId);
        Task<FarmResult> UpdateAsync(UpdateFarmDto farmDto, string userId);
    }
}
