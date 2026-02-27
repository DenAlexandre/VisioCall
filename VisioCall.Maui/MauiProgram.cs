using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using VisioCall.Maui.PageModels;
using VisioCall.Maui.Pages;
using VisioCall.Maui.Services;

namespace VisioCall.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Platform-specific WebView configuration
#if ANDROID
        Platforms.Android.Handlers.WebViewPermissionHandler.Configure();
#elif IOS
        Platforms.iOS.Handlers.WebViewMediaHandler.Configure();
#endif

        // Services
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<SignalingService>();
        builder.Services.AddSingleton<CallService>();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<PermissionService>();
        builder.Services.AddTransient<WebRtcService>();

        // PageModels
        builder.Services.AddTransient<LoginPageModel>();
        builder.Services.AddTransient<HomePageModel>();
        builder.Services.AddTransient<IncomingCallPageModel>();
        builder.Services.AddTransient<OutgoingCallPageModel>();
        builder.Services.AddTransient<VideoCallPageModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<IncomingCallPage>();
        builder.Services.AddTransient<OutgoingCallPage>();
        builder.Services.AddTransient<VideoCallPage>();

        return builder.Build();
    }
}
