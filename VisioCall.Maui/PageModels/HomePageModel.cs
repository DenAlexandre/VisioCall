using System.Collections.ObjectModel;
using VisioCall.Maui.Pages;
using VisioCall.Maui.Services;
using VisioCall.Shared.Models;

namespace VisioCall.Maui.PageModels;

public partial class HomePageModel : ObservableObject
{
    private readonly SignalingService _signaling;
    private readonly CallService _callService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "";

    public ObservableCollection<UserInfo> OnlineUsers { get; } = [];

    public HomePageModel(SignalingService signaling, CallService callService)
    {
        _signaling = signaling;
        _callService = callService;

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

    [RelayCommand]
    private async Task CallUserAsync(UserInfo user)
    {
        var success = await _callService.StartCallAsync(user.UserId);
        if (success)
        {
            await Shell.Current.GoToAsync($"{nameof(OutgoingCallPage)}?remoteUser={user.DisplayName}&remoteUserId={user.UserId}");
        }
        else
        {
            StatusMessage = $"Could not call {user.DisplayName}";
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
