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

        Task<int> AddManagerAsync(string farmId, string userId, DateTimeOffset assigned);
    }
}
