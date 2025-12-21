using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.LLM;
using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.Farms
{
    public interface IFarmRepository
    {
        Task<int> AddAsync(Farm farm);
        Task<Farm?> GetByIdAsync(string farmId);
        Task<FarmDto?> GetFullInfoAsync(string farmId);
        Task<IEnumerable<FarmDto>?> GetByOwnerAsync(string ownerId);
        Task<IEnumerable<FarmDto>?> GetByManagerAsync(string managerId);
        Task<int> UpdateAsync(UpdateFarmDto farmDto, string userId);
        Task<int> DeleteAsync(string farmId, string userId);
        Task<bool> IsUserOwnerAsync(string farmId, string userId);

        // Farm managers
        Task<int> AddManagerAsync(string farmId, string userId, DateTimeOffset assigned);
        Task<int> DeleteManagerAsync(string farmId, string userId);
        Task<bool> IsUserFarmManagerAsync(string farmId, string userId);

        // Farm fields
        Task<int> AddFieldAsync(Field field);
        Task<FieldDto?> GetFieldByIdAsync(string fieldId);
        Task<LlmField?> GetFieldByNameAsync(string fieldName, string farmId);
        Task<IEnumerable<LlmField>?> GetFieldsByFarmAsync(string farmId);
        Task<bool> CheckFieldExists(string fieldName, string farmId);
        Task<bool> UpdateFieldCurrentFarmAsync(string fieldId, string farmId);
        Task<bool> RevertFieldCurrentFarmAsync(string fieldId);
        Task<bool> UpdateFieldAsync(UpdateFieldDto fieldDto);
        Task<bool> UpdateFieldStatusAsync(string fieldId, FieldStatus status);

        // Field cultivations
        Task<int> AddFieldCultivationAsync(FieldCultivation cultivation);
        Task<IEnumerable<FieldCultivationDto>> GetFieldCultivationsAsync(string fieldId);
        Task<FieldCultivationDto?> GetFieldCultivationByIdAsync(string cultivationId);
        Task<bool> UpdateFieldHarvestedAsync(FieldHarvestDto harvestDto);
        Task<bool> UpdateFieldCultivationStatusAsync(string cultivationId, CultivationStatus status);
        Task<bool> DeleteFieldCultivationAsync(string cultivationId);
    }
}
