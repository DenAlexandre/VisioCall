namespace VisioCall.Maui.Services;

public class PermissionService
{
    public async Task<bool> RequestCameraAndMicrophoneAsync()
    {
#if WINDOWS
        // On Windows (packaged WinUI), permissions are declared in Package.appxmanifest.
        // No runtime prompt is needed — the OS grants them at install time.
        await Task.CompletedTask;
        return true;
#else
        var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();

        if (cameraStatus != PermissionStatus.Granted)
            cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

        if (micStatus != PermissionStatus.Granted)
            micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

        return cameraStatus == PermissionStatus.Granted && micStatus == PermissionStatus.Granted;
#endif
    }
}
