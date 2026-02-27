using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace VisioCall.Maui.Platforms.Android.Handlers;

/// <summary>
/// CRITICAL: Without this handler, getUserMedia() fails silently on Android WebView.
/// We must override MauiWebChromeClient to handle OnPermissionRequest.
/// </summary>
public class WebViewPermissionHandler
{
    public static void Configure()
    {
        WebViewHandler.Mapper.AppendToMapping("WebRtcPermissions", (handler, view) =>
        {
            var webView = handler.PlatformView;

            // Enable JavaScript
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.MediaPlaybackRequiresUserGesture = false;
            webView.Settings.AllowFileAccess = true;
            webView.Settings.DomStorageEnabled = true;

            // Set custom WebChromeClient that grants media permissions
            webView.SetWebChromeClient(new VisioCallWebChromeClient());
        });
    }

    private class VisioCallWebChromeClient : global::Android.Webkit.WebChromeClient
    {
        public override void OnPermissionRequest(PermissionRequest? request)
        {
            if (request?.GetResources() is { } resources)
            {
                // Grant all requested permissions (camera, microphone)
                request.Grant(resources);
            }
        }
    }
}
