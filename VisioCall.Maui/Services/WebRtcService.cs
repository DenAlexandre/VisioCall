using System.Text.Json;
using VisioCall.Shared.Models;

namespace VisioCall.Maui.Services;

/// <summary>
/// Bridge between C# and WebRTC JavaScript running in a WebView.
/// C# -> JS: EvaluateJavaScriptAsync
/// JS -> C#: URL interception (visiocall://...) on Android/iOS,
///           postMessage on Windows/WebView2
/// </summary>
public class WebRtcService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private WebView? _webView;
    private readonly SignalingService _signaling;
    private string? _remoteUserId;

    private Action<SessionDescription>? _onReceiveOffer;
    private Action<SessionDescription>? _onReceiveAnswer;
    private Action<IceCandidate>? _onReceiveIceCandidate;

    public event Action<string>? OnError;

    public WebRtcService(SignalingService signaling)
    {
        _signaling = signaling;
    }

    public void AttachWebView(WebView webView, string remoteUserId)
    {
        // Clean up any prior subscriptions to avoid double-handling
        DetachWebView();

        _webView = webView;
        _remoteUserId = remoteUserId;
        _webView.Navigating += OnWebViewNavigating;

#if WINDOWS
        Platforms.Windows.Handlers.WebViewPermissionHandler.OnWebMessageReceived += OnWindowsWebMessage;
#endif

        // Wire SignalR events to JS
        _onReceiveOffer = async offer =>
            await EvalJsAsync($"receiveOffer({JsonSerializer.Serialize(offer, JsonOptions)})");
        _onReceiveAnswer = async answer =>
            await EvalJsAsync($"receiveAnswer({JsonSerializer.Serialize(answer, JsonOptions)})");
        _onReceiveIceCandidate = async candidate =>
            await EvalJsAsync($"receiveIceCandidate({JsonSerializer.Serialize(candidate, JsonOptions)})");

        _signaling.OnReceiveOffer += _onReceiveOffer;
        _signaling.OnReceiveAnswer += _onReceiveAnswer;
        _signaling.OnReceiveIceCandidate += _onReceiveIceCandidate;
    }

    public void DetachWebView()
    {
        if (_onReceiveOffer is not null) _signaling.OnReceiveOffer -= _onReceiveOffer;
        if (_onReceiveAnswer is not null) _signaling.OnReceiveAnswer -= _onReceiveAnswer;
        if (_onReceiveIceCandidate is not null) _signaling.OnReceiveIceCandidate -= _onReceiveIceCandidate;

#if WINDOWS
        Platforms.Windows.Handlers.WebViewPermissionHandler.OnWebMessageReceived -= OnWindowsWebMessage;
#endif

        if (_webView is not null)
        {
            _webView.Navigating -= OnWebViewNavigating;
            _webView = null;
        }
    }

    public async Task CreateOfferAsync() =>
        await EvalJsAsync("createOffer()");

    public async Task CreateAnswerAsync() =>
        await EvalJsAsync("createAnswer()");

    public async Task ToggleMuteAsync() =>
        await EvalJsAsync("toggleMute()");

    public async Task ToggleCameraAsync() =>
        await EvalJsAsync("toggleCamera()");

    public async Task CloseConnectionAsync() =>
        await EvalJsAsync("closeConnection()");

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        Console.WriteLine($"[VisioCall] Navigating: {e.Url}");
        if (!e.Url.StartsWith("visiocall://")) return;
        e.Cancel = true;

        await ProcessVisiocallUrlAsync(e.Url);
    }

#if WINDOWS
    private async void OnWindowsWebMessage(string url)
    {
        Console.WriteLine($"[VisioCall] WebMessage: {url}");
        await ProcessVisiocallUrlAsync(url);
    }
#endif

    private async Task ProcessVisiocallUrlAsync(string url)
    {
        try
        {
            var uri = new Uri(url);
            var action = uri.Host;
            var data = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

            switch (action)
            {
                case "offer":
                    var offer = JsonSerializer.Deserialize<SessionDescription>(data, JsonOptions)!;
                    await _signaling.SendOfferAsync(_remoteUserId!, offer);
                    break;
                case "answer":
                    var answer = JsonSerializer.Deserialize<SessionDescription>(data, JsonOptions)!;
                    await _signaling.SendAnswerAsync(_remoteUserId!, answer);
                    break;
                case "ice-candidate":
                    var candidate = JsonSerializer.Deserialize<IceCandidate>(data, JsonOptions)!;
                    await _signaling.SendIceCandidateAsync(_remoteUserId!, candidate);
                    break;
                case "error":
                    OnError?.Invoke(data);
                    break;
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"WebRTC bridge error: {ex.Message}");
        }
    }

    private async Task EvalJsAsync(string js)
    {
        Console.WriteLine($"[VisioCall] EvalJsAsync: {js[..Math.Min(js.Length, 80)]}");
        if (_webView is null)
        {
            Console.WriteLine("[VisioCall] EvalJsAsync: _webView is NULL!");
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                var result = await _webView.EvaluateJavaScriptAsync(js);
                Console.WriteLine($"[VisioCall] EvalJsAsync result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VisioCall] EvalJsAsync ERROR: {ex.Message}");
                OnError?.Invoke($"JS eval error: {ex.Message}");
            }
        });
    }
}
