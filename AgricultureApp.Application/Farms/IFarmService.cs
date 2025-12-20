using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.Farms
{
    public interface IFarmService
    {
        Task<FarmResult<Farm>> CreateAsync(CreateFarmDto farmDto, string userId);
        Task<FarmResult<FarmDto>> GetFullInfoAsync(string farmId);
        Task<FarmResult<FarmDto>> GetByOwnerAsync(string ownerId);
        Task<FarmResult<FarmDto>> GetByManagerAsync(string managerId);
        Task<FarmResult<Farm>> UpdateAsync(UpdateFarmDto farmDto, string userId);
        Task<BaseResult> DeleteAsync(string farmId, string userId);

        // Farm managers
        Task<ManagerResult> AddManagerAsync(string userId, string farmId, string email);
        Task<BaseResult> DeleteManagerAsync(string farmId, string userId, string managerId);

        // Farm fields
        Task<FieldResult> CreateFieldAsync(CreateFieldDto fieldDto, string userId);
        Task<FieldResult> GetFieldByIdAsync(string fieldId, string userId);
        Task<BaseResult> UpdateFieldCurrentFarmAsync(string fieldId, UpdateFieldFarmDto update, string userId);
        Task<BaseResult> RevertFieldCurrentFarmAsync(string fieldId, UpdateFieldFarmDto update, string userId);
        Task<BaseResult> UpdateFieldAsync(UpdateFieldDto fieldDto, string userId);
        Task<BaseResult> UpdateFieldStatusAsync(UpdateFieldStatusDto fieldStatusDto, string userId);

        // Field cultivations
        Task<FieldCultivationResult> AddFieldCultivationAsync(CreateFieldCultivationDto cultivationDto, string userId);
        Task<FieldCultivationResult> GetFieldCultivationsAsync(string fieldId, string farmId, string userId);
        Task<BaseResult> SetFieldHarvestedAsync(FieldHarvestDto harvestDto, string userId);
        Task<BaseResult> UpdateFieldCultivationStatusAsync(UpdateFieldCultivationStatusDto update, string userId);
        Task<BaseResult> DeleteFieldCultivationAsync(DeleteFieldCultivationDto deleteItems, string userId);

    }
}
