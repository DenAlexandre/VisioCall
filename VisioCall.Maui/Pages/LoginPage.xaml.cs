using VisioCall.Maui.PageModels;

namespace VisioCall.Maui.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}
