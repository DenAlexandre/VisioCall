using VisioCall.Maui.Services;

namespace VisioCall.Maui.PageModels;

[QueryProperty(nameof(RemoteUserId), "remoteUserId")]
[QueryProperty(nameof(RemoteUserName), "remoteUserName")]
[QueryProperty(nameof(IsCallerStr), "isCaller")]
public partial class VideoCallPageModel : ObservableObject
{
    private readonly CallService _callService;
    private readonly WebRtcService _webRtc;

    [ObservableProperty]
    private string _remoteUserId = "";

    [ObservableProperty]
    private string _remoteUserName = "";

    [ObservableProperty]
    private string _isCallerStr = "false";

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private bool _isCameraOff;

    [ObservableProperty]
    private string _callDuration = "00:00";

    private System.Timers.Timer? _durationTimer;
    private int _elapsedSeconds;

    public bool IsCaller => IsCallerStr.Equals("true", StringComparison.OrdinalIgnoreCase);

    public WebRtcService WebRtc => _webRtc;

    public VideoCallPageModel(CallService callService, WebRtcService webRtc)
    {
        _callService = callService;
        _webRtc = webRtc;

        _callService.OnCallEnded += OnCallEnded;
    }

    public async Task InitializeCallAsync(WebView webView)
    {
        _webRtc.AttachWebView(webView, RemoteUserId);

        _callService.SetConnected();
        StartDurationTimer();

        // The caller creates the offer, the callee waits for the offer
        if (IsCaller)
        {
            // Small delay to let WebView load
            await Task.Delay(1500);
            await _webRtc.CreateOfferAsync();
        }
    }

    [RelayCommand]
    private async Task ToggleMuteAsync()
    {
        await _webRtc.ToggleMuteAsync();
        IsMuted = !IsMuted;
    }

    [RelayCommand]
    private async Task ToggleCameraAsync()
    {
        await _webRtc.ToggleCameraAsync();
        IsCameraOff = !IsCameraOff;
    }

    [RelayCommand]
    private async Task HangUpAsync()
    {
        StopDurationTimer();
        await _webRtc.CloseConnectionAsync();
        _webRtc.DetachWebView();
        await _callService.HangUpAsync();
        await Shell.Current.GoToAsync("../..");
    }

    private void OnCallEnded()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            StopDurationTimer();
            await _webRtc.CloseConnectionAsync();
            _webRtc.DetachWebView();
            await Shell.Current.GoToAsync("../..");
        });
    }

    private void StartDurationTimer()
    {
        _elapsedSeconds = 0;
        _durationTimer = new System.Timers.Timer(1000);
        _durationTimer.Elapsed += (_, _) =>
        {
            _elapsedSeconds++;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CallDuration = TimeSpan.FromSeconds(_elapsedSeconds).ToString(@"mm\:ss");
            });
        };
        _durationTimer.Start();
    }

    private void StopDurationTimer()
    {
        _durationTimer?.Stop();
        _durationTimer?.Dispose();
        _durationTimer = null;
    }
}
