using VisioCall.Maui.PageModels;

namespace VisioCall.Maui.Pages;

public partial class VideoCallPage : ContentPage
{
    private readonly VideoCallPageModel _pageModel;
    private bool _initialized;

    public VideoCallPage(VideoCallPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = _pageModel = pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized) return;
        _initialized = true;

        // Load WebRTC HTML into the WebView
        WebRtcView.Source = new HtmlWebViewSource
        {
            Html = await LoadWebRtcHtmlAsync()
        };

        // Wait for the WebView to fully parse and execute the inline JS
        await Task.Delay(1500);

        await _pageModel.InitializeCallAsync(WebRtcView);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _initialized = false;
    }

    private static async Task<string> LoadWebRtcHtmlAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("webrtc/index.html");
        using var reader = new StreamReader(stream);
        var html = await reader.ReadToEndAsync();

        // Inline the CSS and JS
        try
        {
            using var cssStream = await FileSystem.OpenAppPackageFileAsync("webrtc/webrtc.css");
            using var cssReader = new StreamReader(cssStream);
            var css = await cssReader.ReadToEndAsync();
            html = html.Replace("/*INLINE_CSS*/", css);
        }
        catch { }

        try
        {
            using var jsStream = await FileSystem.OpenAppPackageFileAsync("webrtc/webrtc.js");
            using var jsReader = new StreamReader(jsStream);
            var js = await jsReader.ReadToEndAsync();
            html = html.Replace("/*INLINE_JS*/", js);
        }
        catch { }

        return html;
    }
}
