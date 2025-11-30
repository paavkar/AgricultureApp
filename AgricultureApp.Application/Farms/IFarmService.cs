using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.Farms
{
    public interface IFarmService
    {
        Task<FarmResult<Farm>> CreateAsync(CreateFarmDto farmDto, string userId);
        Task<FarmResult<Farm>> GetByIdAsync(string farmId);
        Task<FarmResult<FarmDto>> GetFullInfoAsync(string farmId);
        Task<FarmListResult> GetByOwnerAsync(string ownerId);
        Task<FarmResult<Farm>> UpdateAsync(UpdateFarmDto farmDto, string userId);
        Task<BaseResult> DeleteAsync(string farmId, string userId);

        Task<ManagerResult> AddManagerAsync(string userId, string farmId, string email);
        Task<BaseResult> DeleteManagerAsync(string farmId, string userId, string managerId);
    }
}
