using VisioCall.Maui.Pages;
using VisioCall.Maui.Services;

namespace VisioCall.Maui.PageModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly SignalingService _signaling;
    private readonly PermissionService _permissions;

    [ObservableProperty]
    private string _serverUrl = "https://eradicable-nonincandescent-sachiko.ngrok-free.dev";

    [ObservableProperty]
    private string _userId = "";

    [ObservableProperty]
    private string _displayName = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isBusy;

    public LoginPageModel(SignalingService signaling, PermissionService permissions)
    {
        _signaling = signaling;
        _permissions = permissions;
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(ServerUrl))
        {
            ErrorMessage = "Please fill in all fields.";
            return;
        }

        IsBusy = true;
        ErrorMessage = "";

        try
        {
            var granted = await _permissions.RequestCameraAndMicrophoneAsync();
            if (!granted)
            {
                ErrorMessage = "Camera and microphone permissions are required.";
                return;
            }

            await _signaling.ConnectAsync(ServerUrl);
            var name = string.IsNullOrWhiteSpace(DisplayName) ? UserId : DisplayName;
            await _signaling.RegisterAsync(UserId, name);

            await Shell.Current.GoToAsync(nameof(HomePage));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Connection failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
