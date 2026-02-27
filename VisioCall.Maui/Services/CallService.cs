using VisioCall.Shared.Models;

namespace VisioCall.Maui.Services;

/// <summary>
/// Orchestrates the call lifecycle including auto-answer after 3 rings.
/// </summary>
public class CallService
{
    private readonly SignalingService _signaling;
    private readonly AudioService _audio;
    private CancellationTokenSource? _autoAnswerCts;

    public CallState State { get; private set; }
    public string? RemoteUserId { get; private set; }
    public string? RemoteUserName { get; private set; }

    public event Action<CallState>? OnStateChanged;
    public event Action<CallRequest>? OnIncomingCall;
    public event Action? OnCallAccepted;
    public event Action? OnCallEnded;

    public CallService(SignalingService signaling, AudioService audio)
    {
        _signaling = signaling;
        _audio = audio;

        _signaling.OnIncomingCall += HandleIncomingCall;
        _signaling.OnCallResponseReceived += HandleCallResponse;
        _signaling.OnCallEnded += HandleCallEnded;
    }

    public async Task<bool> StartCallAsync(string calleeId)
    {
        var response = await _signaling.InitiateCallAsync(calleeId);
        if (!response.Success) return false;

        RemoteUserId = calleeId;
        SetState(CallState.Calling);
        return true;
    }

    public async Task AcceptCallAsync()
    {
        StopAutoAnswer();
        _audio.StopRinging();

        if (RemoteUserId is null) return;

        await _signaling.RespondToCallAsync(RemoteUserId, true);
        SetState(CallState.Connecting);
        OnCallAccepted?.Invoke();
    }

    public async Task RejectCallAsync()
    {
        StopAutoAnswer();
        _audio.StopRinging();

        if (RemoteUserId is null) return;

        await _signaling.RespondToCallAsync(RemoteUserId, false);
        SetState(CallState.Ended);
        RemoteUserId = null;
        OnCallEnded?.Invoke();
    }

    public async Task HangUpAsync()
    {
        StopAutoAnswer();
        _audio.StopRinging();

        if (RemoteUserId is not null)
        {
            await _signaling.EndCallAsync(RemoteUserId);
        }

        SetState(CallState.Ended);
        RemoteUserId = null;
        OnCallEnded?.Invoke();
    }

    public void SetConnected() => SetState(CallState.Connected);

    private void HandleIncomingCall(CallRequest request)
    {
        RemoteUserId = request.CallerId;
        RemoteUserName = request.CallerName;
        SetState(CallState.Ringing);

        OnIncomingCall?.Invoke(request);
        StartAutoAnswer();
    }

    private void HandleCallResponse(bool accepted)
    {
        if (accepted)
        {
            SetState(CallState.Connecting);
            OnCallAccepted?.Invoke();
        }
        else
        {
            SetState(CallState.Ended);
            RemoteUserId = null;
            OnCallEnded?.Invoke();
        }
    }

    private void HandleCallEnded()
    {
        StopAutoAnswer();
        _audio.StopRinging();
        SetState(CallState.Ended);
        RemoteUserId = null;
        OnCallEnded?.Invoke();
    }

    private void StartAutoAnswer()
    {
        _autoAnswerCts = new CancellationTokenSource();
        var token = _autoAnswerCts.Token;

        _ = Task.Run(async () =>
        {
            // Start ringing: 3 rings x 5s = 15s
            await _audio.StartRingingAsync();

            if (token.IsCancellationRequested) return;

            // Auto-answer after 3 rings
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (State == CallState.Ringing)
                {
                    await AcceptCallAsync();
                }
            });
        }, token);
    }

    private void StopAutoAnswer()
    {
        _autoAnswerCts?.Cancel();
        _autoAnswerCts?.Dispose();
        _autoAnswerCts = null;
    }

    private void SetState(CallState state)
    {
        State = state;
        OnStateChanged?.Invoke(state);
    }
}
