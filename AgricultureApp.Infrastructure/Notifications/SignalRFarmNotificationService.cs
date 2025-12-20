using AgricultureApp.Application.DTOs;
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

        public async Task NotifyFieldUpdatedAsync(string farmId, UpdateFieldDto arg, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldUpdated", arg, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }

        public async Task NotifyFieldCultChangeAsync(string farmId, UpdateFieldFarmDto arg, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldCultivatorChanged", arg, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }

        public async Task NotifyFieldStatusChangedAsync(string farmId, UpdateFieldStatusDto arg, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldStatusChanged", arg, cancellationToken);
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

        public async Task NotifyFieldHarvestedAsync(string farmId, FieldHarvestDto arg, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldHarvested", arg, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }

        public async Task NotifyFieldCultivationStatusUpdatedAsync(string farmId, UpdateFieldCultivationStatusDto arg, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldCultivationUpdated", arg, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }

        public async Task NotifyFieldCultivationDeletedAsync(string farmId, string cultivationId, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.SendToGroupAsync(farmId, "FieldCultivationDeleted", cultivationId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to notify farm {FarmId} about added field.", farmId);
            }
        }
    }
}
