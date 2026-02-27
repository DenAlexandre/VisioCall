using VisioCall.Maui.Pages;
using VisioCall.Maui.Services;

namespace VisioCall.Maui.PageModels;

[QueryProperty(nameof(CallerName), "callerName")]
[QueryProperty(nameof(CallerId), "callerId")]
public partial class IncomingCallPageModel : ObservableObject
{
    private readonly CallService _callService;

    [ObservableProperty]
    private string _callerName = "";

    [ObservableProperty]
    private string _callerId = "";

    [ObservableProperty]
    private int _ringCount;

    [ObservableProperty]
    private string _statusText = "Incoming call...";

    public IncomingCallPageModel(CallService callService)
    {
        _callService = callService;

        _callService.OnCallAccepted += OnCallAccepted;
        _callService.OnCallEnded += OnCallEnded;
        _callService.OnStateChanged += OnStateChanged;
    }

    [RelayCommand]
    private async Task AcceptAsync()
    {
        await _callService.AcceptCallAsync();
    }

    [RelayCommand]
    private async Task RejectAsync()
    {
        await _callService.RejectCallAsync();
        await Shell.Current.GoToAsync("..");
    }

    private void OnCallAccepted()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync($"../{nameof(VideoCallPage)}?remoteUserId={CallerId}&remoteUserName={CallerName}&isCaller=false");
        });
    }

    private void OnCallEnded()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("..");
        });
    }

    private void OnStateChanged(Shared.Models.CallState state)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusText = state switch
            {
                Shared.Models.CallState.Ringing => $"Incoming call from {CallerName}...",
                Shared.Models.CallState.Connecting => "Connecting...",
                _ => StatusText
            };
        });
    }
}
