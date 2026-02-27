using Foundation;
using Microsoft.Maui.Handlers;
using WebKit;

namespace VisioCall.Maui.Platforms.iOS.Handlers;

/// <summary>
/// Configures WKWebView to allow media capture (camera/microphone) on iOS.
/// </summary>
public class WebViewMediaHandler
{
    public static void Configure()
    {
        WebViewHandler.Mapper.AppendToMapping("WebRtcMediaiOS", (handler, view) =>
        {
            if (handler.PlatformView is WKWebView wkWebView)
            {
                // Enable inline media playback
                var config = wkWebView.Configuration;
                config.AllowsInlineMediaPlayback = true;
                config.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
            }
        });
    }
}
