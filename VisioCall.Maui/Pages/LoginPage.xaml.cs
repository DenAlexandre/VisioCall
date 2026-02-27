using VisioCall.Maui.PageModels;

namespace VisioCall.Maui.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageModel _pageModel;

    public LoginPage(LoginPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = _pageModel = pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Auto-connect if credentials are already saved
        if (_pageModel.HasSavedCredentials)
        {
            await _pageModel.ConnectCommand.ExecuteAsync(null);
        }
    }
}
