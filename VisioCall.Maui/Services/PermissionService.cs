namespace VisioCall.Maui.Services;

public class PermissionService
{
    public async Task<bool> RequestCameraAndMicrophoneAsync()
    {
        var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();

        if (cameraStatus != PermissionStatus.Granted)
            cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

        if (micStatus != PermissionStatus.Granted)
            micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

        return cameraStatus == PermissionStatus.Granted && micStatus == PermissionStatus.Granted;
    }
}
