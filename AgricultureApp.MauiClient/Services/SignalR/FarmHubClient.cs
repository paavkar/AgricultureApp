using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.MauiClient.Services
{
    public class FarmHubClient : IFarmHubClient
    {
        private readonly HubConnection _connection;
        private readonly AuthenticationService _auth;
        private readonly ILogger<FarmHubClient> _logger;

        public event EventHandler<string>? UserAddedToFarm;
        public event EventHandler<string>? UserRemovedFromFarm;
        public event EventHandler<Field>? FieldAdded;
        public event EventHandler<FieldUpdate>? FieldUpdated;
        public event EventHandler<StreamingChatMessageContent>? LlmStreamingResponse;
        public event EventHandler<string>? LlmStreamingError;
        public event EventHandler? LlmStreamingFinished;

        public HubConnectionState ConnectionState => _connection.State;

        public FarmHubClient(
        AuthenticationService auth,
        ILogger<FarmHubClient> logger)
        {
            _auth = auth;
            _logger = logger;

            var url = $"{Constants.BaseUrl}/farmhub";
            _connection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        var token = await _auth.GetAccessTokenAsync();

                        if (string.IsNullOrWhiteSpace(token))
                        {
                            _logger.LogWarning("No access token available for SignalR connection.");
                        }
                        return token;
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();
            RegisterLifecycleHandlers();
        }

        private void RegisterHandlers()
        {
            _connection.On<string>("UserAddedToFarm", farmId =>
            {
                _logger.LogInformation("UserAddedToFarm for farm {FarmId}", farmId);
                UserAddedToFarm?.Invoke(this, farmId);
            });

            _connection.On<string>("UserRemovedFromFarm", farmId =>
            {
                _logger.LogInformation("UserRemovedFromFarm for farm {FarmId}", farmId);
                UserRemovedFromFarm?.Invoke(this, farmId);
            });

            _connection.On<Field>("FieldAdded", field =>
            {
                FieldAdded?.Invoke(this, field);
            });

            _connection.On<FieldUpdate>("FieldUpdated", field =>
            {
                FieldUpdated?.Invoke(this, field);
            });

            _connection.On<StreamingChatMessageContent>("LlmStreamingResponse", token =>
            {
                LlmStreamingResponse?.Invoke(this, token);
            });

            _connection.On<string>("LlmStreamingError", message =>
            {
                LlmStreamingError?.Invoke(this, message);
            });

            _connection.On("LlmStreamingFinished", () =>
            {
                LlmStreamingFinished?.Invoke(this, EventArgs.Empty);
            });
        }

        private void RegisterLifecycleHandlers()
        {
            _connection.Reconnecting += error =>
            {
                _logger.LogWarning(error, "SignalR reconnecting...");
                return Task.CompletedTask;
            };

            _connection.Reconnected += connectionId =>
            {
                _logger.LogInformation("SignalR reconnected. ConnectionId: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _connection.Closed += async error =>
            {
                _logger.LogWarning(error, "SignalR connection closed.");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await ConnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to restart SignalR connection after close.");
                }
            };
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (_connection.State == HubConnectionState.Connected ||
                _connection.State == HubConnectionState.Connecting)
            {
                return;
            }

            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogInformation("Skipping SignalR connect: no token.");
                return;
            }

            try
            {
                _logger.LogInformation("Connecting… Current state: {State}", _connection.State);

                await _connection.StartAsync(cancellationToken);

                _logger.LogInformation("Connected. New state: {State}", _connection.State);
                _logger.LogInformation("ConnectionId: {Id}", _connection.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting SignalR connection.");
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                return;
            }

            try
            {
                await _connection.StopAsync(cancellationToken);
                _logger.LogInformation("SignalR disconnected.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping SignalR connection.");
            }
        }

        public async Task JoinFarmAsync(string farmId, CancellationToken cancellationToken = default)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                await ConnectAsync(cancellationToken);
            }

            try
            {
                await _connection.InvokeAsync("JoinFarm", farmId, cancellationToken);
                _logger.LogInformation("Joined farm group {FarmId}", farmId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to join farm {FarmId}", farmId);
            }
        }

        public async Task LeaveGroupAsync(string groupName, CancellationToken cancellationToken = default)
        {
            if (_connection.State != HubConnectionState.Connected)
                return;

            try
            {
                await _connection.InvokeAsync("LeaveGroup", groupName, cancellationToken);
                _logger.LogInformation($"Left group with name: {groupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to leave group {GroupName}", groupName);
            }
        }

        public async Task JoinGroupAsync(string groupName, CancellationToken cancellationToken = default)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                await ConnectAsync(cancellationToken);
            }

            try
            {
                await _connection.InvokeAsync("JoinGroup", groupName, cancellationToken);
                _logger.LogInformation($"Joined group with name: {groupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to join group {GroupName}", groupName);
            }
        }

    }
}
