using VisioCall.Maui.PageModels;

namespace VisioCall.Maui.Pages;

public partial class HomePage : ContentPage
{
    private readonly HomePageModel _pageModel;

    public HomePage(HomePageModel pageModel)
    {
        InitializeComponent();
        BindingContext = _pageModel = pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _pageModel.LoadUsersCommand.ExecuteAsync(null);
    }
}
