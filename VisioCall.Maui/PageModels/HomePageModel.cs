using System.Collections.ObjectModel;
using VisioCall.Maui.Models;
using VisioCall.Maui.Pages;
using VisioCall.Maui.Services;
using VisioCall.Shared.Models;

namespace VisioCall.Maui.PageModels;

public partial class HomePageModel : ObservableObject
{
    private readonly SignalingService _signaling;
    private readonly CallService _callService;
    private readonly CallHistoryService _historyService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "";

    public string CurrentUserName
    {
        get
        {
            var display = Preferences.Get("DisplayName", "");
            var userId = Preferences.Get("UserId", "");
            return string.IsNullOrWhiteSpace(display) ? userId : display;
        }
    }

    public ObservableCollection<UserInfo> OnlineUsers { get; } = [];
    public ObservableCollection<CallHistoryEntry> CallHistory { get; } = [];

    public HomePageModel(SignalingService signaling, CallService callService, CallHistoryService historyService)
    {
        _signaling = signaling;
        _callService = callService;
        _historyService = historyService;

        _signaling.OnUserStatusChanged += OnUserStatusChanged;
        _callService.OnIncomingCall += OnIncomingCall;
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        IsBusy = true;
        try
        {
            var users = await _signaling.GetOnlineUsersAsync();
            OnlineUsers.Clear();
            foreach (var user in users)
                OnlineUsers.Add(user);

            LoadHistory();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadHistory()
    {
        CallHistory.Clear();
        foreach (var entry in _historyService.GetHistory())
            CallHistory.Add(entry);
    }

    [RelayCommand]
    private async Task CallUserAsync(UserInfo user)
    {
        await PlaceCallAsync(user.UserId, user.DisplayName);
    }

    [RelayCommand]
    private async Task RecallAsync(CallHistoryEntry entry)
    {
        await PlaceCallAsync(entry.UserId, entry.DisplayName);
    }

    private async Task PlaceCallAsync(string userId, string displayName)
    {
        var success = await _callService.StartCallAsync(userId, displayName);
        if (success)
        {
            await Shell.Current.GoToAsync($"{nameof(OutgoingCallPage)}?remoteUser={displayName}&remoteUserId={userId}");
        }
        else
        {
            StatusMessage = $"Could not call {displayName}";
        }
    }

    private void OnUserStatusChanged(UserInfo user)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var existing = OnlineUsers.FirstOrDefault(u => u.UserId == user.UserId);
            if (existing is not null)
                OnlineUsers.Remove(existing);

            if (user.IsOnline)
                OnlineUsers.Add(user);
        });
    }

    private void OnIncomingCall(CallRequest request)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync($"{nameof(IncomingCallPage)}?callerName={request.CallerName}&callerId={request.CallerId}");
        });
    }
}
