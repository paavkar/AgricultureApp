using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Farms;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.Infrastructure.Farms
{
    public class FarmService(
        ILogger<FarmService> logger,
        IFarmRepository farmRepository) : IFarmService
    {
        public async Task<FarmResult> CreateAsync(CreateFarmDto farmDto, string userId)
        {
            Farm farm = farmDto.ToFarmModel(farmDto, userId);
            var rownsAffected = await farmRepository.AddAsync(farm);

            if (rownsAffected == 0)
            {
                logger.LogError("Failed to create farm for owner {OwnerId}", userId);
                return new FarmResult
                {
                    Succeeded = false,
                    Errors = ["Failed to create farm."]
                };
            }

            logger.LogInformation("Successfully created farm {FarmName} - {FarmId} for owner {OwnerId}",
                farm.Name, farm.Id, farm.OwnerId);
            return new FarmResult
            {
                Succeeded = true,
                Farm = farm
            };
        }
        public Task<FarmResult> GetByIdAsync(string farmId) => throw new NotImplementedException();
        public Task<FarmListResult> GetByOwnerAsync(string ownerId) => throw new NotImplementedException();
    }
}
