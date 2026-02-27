using VisioCall.Maui.Pages;
using VisioCall.Maui.Services;

namespace VisioCall.Maui.PageModels;

[QueryProperty(nameof(RemoteUser), "remoteUser")]
[QueryProperty(nameof(RemoteUserId), "remoteUserId")]
public partial class OutgoingCallPageModel : ObservableObject
{
    private readonly CallService _callService;

    [ObservableProperty]
    private string _remoteUser = "";

    [ObservableProperty]
    private string _remoteUserId = "";

    [ObservableProperty]
    private string _statusText = "Calling...";

    public OutgoingCallPageModel(CallService callService)
    {
        _callService = callService;

        _callService.OnCallAccepted += OnCallAccepted;
        _callService.OnCallEnded += OnCallEnded;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _callService.HangUpAsync();
        await Shell.Current.GoToAsync("..");
    }

    private void OnCallAccepted()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync($"../{nameof(VideoCallPage)}?remoteUserId={RemoteUserId}&remoteUserName={RemoteUser}&isCaller=true");
        });
    }

    private void OnCallEnded()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            StatusText = "Call ended";
            await Shell.Current.GoToAsync("..");
        });
    }
}
