using Microsoft.Maui.Handlers;
using Microsoft.Web.WebView2.Core;

namespace VisioCall.Maui.Platforms.Windows.Handlers;

/// <summary>
/// CRITICAL: Without this handler, getUserMedia() fails on WebView2.
/// We must listen to PermissionRequested and auto-grant camera/microphone.
/// Also bridges JS postMessage to C# for the visiocall:// scheme.
/// </summary>
public class WebViewPermissionHandler
{
    /// <summary>
    /// Fired when JavaScript sends a visiocall:// message via postMessage.
    /// WebRtcService subscribes to this on Windows.
    /// </summary>
    public static event Action<string>? OnWebMessageReceived;

    public static void Configure()
    {
        WebViewHandler.Mapper.AppendToMapping("WebRtcPermissions", (handler, view) =>
        {
            var webView2 = handler.PlatformView;

            webView2.CoreWebView2Initialized += (s, e) =>
            {
                if (webView2.CoreWebView2 is { } coreWebView2)
                {
                    coreWebView2.PermissionRequested += (sender, args) =>
                    {
                        if (args.PermissionKind == CoreWebView2PermissionKind.Camera ||
                            args.PermissionKind == CoreWebView2PermissionKind.Microphone)
                        {
                            args.State = CoreWebView2PermissionState.Allow;
                        }
                    };

                    coreWebView2.WebMessageReceived += (sender, args) =>
                    {
                        var message = args.TryGetWebMessageAsString();
                        if (message?.StartsWith("visiocall://") == true)
                        {
                            OnWebMessageReceived?.Invoke(message);
                        }
                    };
                }
            };
        });
    }
}
