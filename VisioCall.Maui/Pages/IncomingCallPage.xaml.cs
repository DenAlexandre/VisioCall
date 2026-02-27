using VisioCall.Maui.PageModels;

namespace VisioCall.Maui.Pages;

public partial class IncomingCallPage : ContentPage
{
    public IncomingCallPage(IncomingCallPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}
