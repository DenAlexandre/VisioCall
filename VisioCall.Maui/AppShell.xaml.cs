using VisioCall.Maui.Pages;

namespace VisioCall.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(IncomingCallPage), typeof(IncomingCallPage));
        Routing.RegisterRoute(nameof(OutgoingCallPage), typeof(OutgoingCallPage));
        Routing.RegisterRoute(nameof(VideoCallPage), typeof(VideoCallPage));
    }
}
