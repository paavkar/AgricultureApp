using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.Notifications
{
    public interface IFarmNotificationService
    {
        Task NotifyUserAddedToFarmAsync(string userId, string farmId, CancellationToken cancellationToken = default);
        Task NotifyUserRemovedFromFarmAsync(string userId, string farmId, CancellationToken cancellationToken = default);
        Task NotifyFieldAddedAsync(string farmId, FieldDto field, CancellationToken cancellationToken = default);
        Task NotifyFieldUpdatedAsync(string farmId, FieldDto field, CancellationToken cancellationToken = default);
        Task NotifyFieldCultivationAddedAsync(string farmId, FieldCultivationDto cultivation, CancellationToken cancellationToken = default);
        Task NotifyFieldCultivationUpdatedAsync(string farmId, FieldCultivationDto cultivation, CancellationToken cancellationToken = default);
        Task NotifyFieldCultivationDeletedAsync(string farmId, FieldCultivationDto cultivationfield, CancellationToken cancellationToken = default);
    }
}
