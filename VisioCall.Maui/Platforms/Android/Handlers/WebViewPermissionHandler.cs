using Android.Util;
using Android.Webkit;
using Microsoft.Maui.Handlers;

namespace VisioCall.Maui.Platforms.Android.Handlers;

/// <summary>
/// CRITICAL: Without this handler, getUserMedia() fails silently on Android WebView.
/// We must override WebChromeClient to handle OnPermissionRequest.
/// We use Post() to ensure our client is set AFTER MAUI's default one.
/// </summary>
public class WebViewPermissionHandler
{
    private const string Tag = "VisioCall";

    public static void Configure()
    {
        WebViewHandler.Mapper.AppendToMapping("WebRtcPermissions", (handler, view) =>
        {
            var webView = handler.PlatformView;

            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.MediaPlaybackRequiresUserGesture = false;
            webView.Settings.AllowFileAccess = true;
            webView.Settings.DomStorageEnabled = true;

            // Post() ensures this runs AFTER MAUI sets its own MauiWebChromeClient,
            // so our override actually sticks.
            webView.Post(() =>
            {
                webView.SetWebChromeClient(new VisioCallWebChromeClient());
                Log.Info(Tag, "WebChromeClient set for WebRTC permissions");
            });
        });
    }

    private class VisioCallWebChromeClient : WebChromeClient
    {
        public override void OnPermissionRequest(PermissionRequest? request)
        {
            if (request?.GetResources() is { } resources)
            {
                Log.Info(Tag, $"Granting WebView permissions: {string.Join(", ", resources)}");
                request.Grant(resources);
            }
        }

        public override bool OnConsoleMessage(ConsoleMessage? consoleMessage)
        {
            if (consoleMessage is not null)
            {
                var msg = $"[JS] {consoleMessage.Message()} (line {consoleMessage.LineNumber()})";
                var level = consoleMessage.InvokeMessageLevel();
                if (level == ConsoleMessage.MessageLevel.Error)
                    Log.Error(Tag, msg);
                else if (level == ConsoleMessage.MessageLevel.Warning)
                    Log.Warn(Tag, msg);
                else
                    Log.Info(Tag, msg);
            }
            return true;
        }
    }
}
