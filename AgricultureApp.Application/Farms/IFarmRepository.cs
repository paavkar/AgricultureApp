using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.Farms
{
    public interface IFarmRepository
    {
        Task<int> AddAsync(Farm farm);
        Task<Farm?> GetByIdAsync(string farmId);
        Task<IEnumerable<Farm>?> GetByOwnerAsync(string ownerId);
    }
}
