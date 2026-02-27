using Plugin.Maui.Audio;

namespace VisioCall.Maui.Services;

public class AudioService : IDisposable
{
    private readonly IAudioManager _audioManager;
    #pragma warning disable CS0414
    private IAudioPlayer? _player = null;
    #pragma warning restore CS0414
    private CancellationTokenSource? _ringCts;

    public int RingCount { get; private set; }

    public event Action<int>? OnRing;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public async Task StartRingingAsync()
    {
        RingCount = 0;
        _ringCts = new CancellationTokenSource();

        try
        {
            while (!_ringCts.Token.IsCancellationRequested && RingCount < 3)
            {
                RingCount++;
                OnRing?.Invoke(RingCount);

                // Play a short beep sound — we use Vibration as a fallback
                try
                {
                    Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
                }
                catch
                {
                    // Vibration might not be available on all devices
                }

                await Task.Delay(5000, _ringCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when stopping
        }
    }

    public void StopRinging()
    {
        _ringCts?.Cancel();
        _player?.Stop();
        try { Vibration.Default.Cancel(); } catch { }
    }

    public void Dispose()
    {
        StopRinging();
        _player?.Dispose();
        _ringCts?.Dispose();
    }
}
