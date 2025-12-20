using AgricultureApp.Application.DTOs;
using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.Notifications
{
    public interface IFarmNotificationService
    {
        Task NotifyUserAddedToFarmAsync(string userId, string farmId, CancellationToken cancellationToken = default);
        Task NotifyUserRemovedFromFarmAsync(string userId, string farmId, CancellationToken cancellationToken = default);
        Task NotifyFieldAddedAsync(string farmId, FieldDto field, CancellationToken cancellationToken = default);
        Task NotifyFieldUpdatedAsync(string farmId, UpdateFieldDto arg, CancellationToken cancellationToken = default);
        Task NotifyFieldCultChangeAsync(string farmId, UpdateFieldFarmDto arg, CancellationToken cancellationToken = default);
        Task NotifyFieldStatusChangedAsync(string farmId, UpdateFieldStatusDto arg, CancellationToken cancellationToken = default);
        Task NotifyFieldCultivationAddedAsync(string farmId, FieldCultivationDto cultivation, CancellationToken cancellationToken = default);
        Task NotifyFieldHarvestedAsync(string farmId, FieldHarvestDto arg, CancellationToken cancellationToken = default);
        Task NotifyFieldCultivationStatusUpdatedAsync(string farmId, UpdateFieldCultivationStatusDto arg, CancellationToken cancellationToken = default);
        Task NotifyFieldCultivationDeletedAsync(string farmId, string cultivationId, CancellationToken cancellationToken = default);
    }
}
