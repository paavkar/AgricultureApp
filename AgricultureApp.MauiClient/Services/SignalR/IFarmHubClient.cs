using Microsoft.AspNetCore.SignalR.Client;

namespace AgricultureApp.MauiClient.Services
{
    public interface IFarmHubClient
    {
        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        Task JoinFarmAsync(string farmId, CancellationToken cancellationToken = default);
        Task LeaveGroupAsync(string groupName, CancellationToken cancellationToken = default);
        Task JoinGroupAsync(string groupName, CancellationToken cancellationToken = default);

        // Events the UI can subscribe to
        event EventHandler<string>? UserAddedToFarm;
        event EventHandler<string>? UserRemovedFromFarm;
        event EventHandler<Field>? FieldAdded;
        event EventHandler<FieldUpdate>? FieldUpdated;
        event EventHandler<StreamingChatMessageContent>? LlmStreamingResponse;
        event EventHandler<string>? LlmStreamingError;
        event EventHandler? LlmStreamingFinished;

        HubConnectionState ConnectionState { get; }
    }
}
