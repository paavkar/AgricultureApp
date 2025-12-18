using AgricultureApp.Application.DTOs;
using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.Farms
{
    public interface IFarmRepository
    {
        Task<int> AddAsync(Farm farm);
        Task<Farm?> GetByIdAsync(string farmId);
        Task<FarmDto?> GetFullInfoAsync(string farmId);
        Task<IEnumerable<Farm>?> GetByOwnerAsync(string ownerId);
        Task<int> UpdateAsync(UpdateFarmDto farmDto, string userId);
        Task<int> DeleteAsync(string farmId, string userId);

        // Farm managers
        Task<int> AddManagerAsync(string farmId, string userId, DateTimeOffset assigned);
        Task<int> DeleteManagerAsync(string farmId, string userId);

        // Farm fields
        Task<int> AddFieldAsync(Field field);
        Task<bool> CheckFieldExists(string fieldName, string farmId);
        Task<bool> UpdateFieldCurrentFarmAsync(string fieldId, string farmId);
        Task<bool> RevertFieldCurrentFarmAsync(string fieldId);
        Task<bool> UpdateFieldAsync(UpdateFieldDto fieldDto);
        Task<bool> UpdateFieldStatusAsync(string fieldId, FieldStatus status);
    }
}
