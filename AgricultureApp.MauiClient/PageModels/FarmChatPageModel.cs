using AgricultureApp.MauiClient.Models.Chat;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace AgricultureApp.MauiClient.PageModels;

public partial class FarmChatPageModel : ObservableObject, IQueryAttributable
{
    private readonly IFarmHubClient _farmHubClient;
    private readonly ChatService _chatService;
    private readonly ILogger<FarmChatPageModel> _logger;

    [ObservableProperty]
    private Farm? farm;

    [ObservableProperty]
    private string userMessage = string.Empty;

    [ObservableProperty]
    private bool isStreaming;

    [ObservableProperty]
    private bool isAssistantTyping;

    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> _messages = [];

    private string chatId = string.Empty;

    public event Action? ScrollToBottomRequested;
    public event Action? CloseRequested;

    public FarmChatPageModel(
        IFarmHubClient farmHubClient,
        ChatService chatService,
        ILogger<FarmChatPageModel> logger)
    {
        _farmHubClient = farmHubClient;

        _farmHubClient.LlmStreamingResponse += OnStreamingResponse;
        _farmHubClient.LlmStreamingError += OnStreamingError;
        _farmHubClient.LlmStreamingFinished += OnStreamingFinished;

        _chatService = chatService;
        _logger = logger;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Farm", out var value))
            Farm = value as Farm;
    }

    private void OnStreamingResponse(object? sender, StreamingChatMessageContent token)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsAssistantTyping = true;

            ChatMessageViewModel? existing = Messages.FirstOrDefault(m => m.MessageId == token.MessageId);

            if (existing == null)
            {
                Messages.Add(new ChatMessageViewModel
                {
                    MessageId = token.MessageId,
                    Author = token.AuthorName ?? token.Role?.Label ?? "assistant",
                    Markdown = token.Content
                });
            }
            else
            {
                existing.Markdown += token.Content;
            }

            ScrollToBottomRequested?.Invoke();
        });
    }

    private void OnStreamingError(object? sender, string error)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Messages.Add(new ChatMessageViewModel
            {
                Author = "system",
                Markdown = $"{error}"
            });
        });
    }

    private void OnStreamingFinished(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsAssistantTyping = false;
            IsStreaming = false;
        });
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(UserMessage))
            return;

        var message = UserMessage;
        UserMessage = string.Empty;

        Messages.Add(new ChatMessageViewModel
        {
            Author = "you",
            Markdown = message
        });

        ScrollToBottomRequested?.Invoke();

        IsStreaming = true;
        IsAssistantTyping = true;

        Message apiMessage = new()
        {
            Content = message,
            FarmId = Farm.Id
        };

        var responseStarted = await _chatService.SendMessageAsync(apiMessage, chatId);

        if (!responseStarted)
        {
            IsStreaming = false;
            IsAssistantTyping = false;

            Messages.Add(new ChatMessageViewModel
            {
                Author = "system",
                Markdown = AppResources.ErrorInSendingMessage
            });
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private async Task Appearing()
    {
        if (string.IsNullOrWhiteSpace(chatId))
        {
            chatId = await _chatService.CreateChatAsync();

            if (string.IsNullOrWhiteSpace(chatId))
            {
                return;
            }
            await _farmHubClient.JoinGroupAsync(chatId);
            Messages.Add(new ChatMessageViewModel
            {
                Author = "assistant",
                Markdown = AppResources.FirstAssistantMessage
            });
        }
    }

    public void Dispose()
    {
        _farmHubClient.LlmStreamingResponse -= OnStreamingResponse;
        _farmHubClient.LlmStreamingError -= OnStreamingError;
        _farmHubClient.LlmStreamingFinished -= OnStreamingFinished;
    }
}

public partial class ChatMessageViewModel : ObservableObject
{
    public string MessageId { get; set; }
    public string Author { get; set; } = "";
    public string Markdown
    {
        get => _markdown;
        set => SetProperty(ref _markdown, value);
    }
    private string _markdown;
}