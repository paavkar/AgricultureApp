namespace AgricultureApp.Application.Notifications
{
    public interface IFarmHubContext
    {
        Task SendToUserAsync(string userId, string method, string arg, CancellationToken cancellationToken = default);
        Task SendToGroupAsync(string groupName, string method, object? arg, CancellationToken cancellationToken = default);
    }
}
