using VisioCall.Maui.PageModels;

namespace VisioCall.Maui.Pages;

public partial class OutgoingCallPage : ContentPage
{
    public OutgoingCallPage(OutgoingCallPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}
