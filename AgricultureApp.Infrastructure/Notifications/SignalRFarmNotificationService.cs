using AgricultureApp.Application.Notifications;
using AgricultureApp.Domain.Farms;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.Infrastructure.Notifications
{
    public class SignalRFarmNotificationService(
        IFarmHubContext hubContext,
        ILogger<SignalRFarmNotificationService> logger) : IFarmNotificationService
    {
        public async Task NotifyUserAddedToFarmAsync(string userId, string farmId, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToUserAsync(userId, "UserAddedToFarm", farmId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify user {UserId} about being added to farm {FarmId}.", userId, farmId);
            }
        }

        public async Task NotifyUserRemovedFromFarmAsync(string userId, string farmId, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToUserAsync(userId, "UserRemovedFromFarm", farmId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify user {UserId} about being removed from farm {FarmId}.", userId, farmId);
            }
        }

        public async Task NotifyFieldAddedAsync(string farmId, FieldDto field, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldAdded", field, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }

        public async Task NotifyFieldUpdatedAsync(string farmId, FieldDto field, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldUpdated", field, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }

        public async Task NotifyFieldCultivationAddedAsync(string farmId, FieldCultivationDto cultivation, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldCultivationAdded", cultivation, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }

        public async Task NotifyFieldCultivationUpdatedAsync(string farmId, FieldCultivationDto cultivation, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldCultivationUpdated", cultivation, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }

        public async Task NotifyFieldCultivationDeletedAsync(string farmId, FieldCultivationDto cultivation, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldCultivationDeleted", cultivation, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }
    }
}
