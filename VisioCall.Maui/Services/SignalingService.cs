using Microsoft.AspNetCore.SignalR.Client;
using VisioCall.Shared.Constants;
using VisioCall.Shared.Models;

namespace VisioCall.Maui.Services;

public class SignalingService : IAsyncDisposable
{
    private HubConnection? _hub;

    public bool IsConnected => _hub?.State == HubConnectionState.Connected;

    // Events from server
    public event Action<CallRequest>? OnIncomingCall;
    public event Action<bool>? OnCallResponseReceived;
    public event Action? OnCallEnded;
    public event Action<SessionDescription>? OnReceiveOffer;
    public event Action<SessionDescription>? OnReceiveAnswer;
    public event Action<IceCandidate>? OnReceiveIceCandidate;
    public event Action<UserInfo>? OnUserStatusChanged;

    public async Task ConnectAsync(string serverUrl)
    {
        if (_hub is not null)
            await DisposeAsync();

        _hub = new HubConnectionBuilder()
            .WithUrl($"{serverUrl.TrimEnd('/')}{HubRoutes.CallHub}")
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();

        await _hub.StartAsync();
    }

    private void RegisterHandlers()
    {
        _hub!.On<CallRequest>(HubRoutes.IncomingCall, req =>
            OnIncomingCall?.Invoke(req));

        _hub!.On<bool>(HubRoutes.CallResponseReceived, accepted =>
            OnCallResponseReceived?.Invoke(accepted));

        _hub!.On(HubRoutes.CallEnded, () =>
            OnCallEnded?.Invoke());

        _hub!.On<SessionDescription>(HubRoutes.ReceiveOffer, offer =>
            OnReceiveOffer?.Invoke(offer));

        _hub!.On<SessionDescription>(HubRoutes.ReceiveAnswer, answer =>
            OnReceiveAnswer?.Invoke(answer));

        _hub!.On<IceCandidate>(HubRoutes.ReceiveIceCandidate, candidate =>
            OnReceiveIceCandidate?.Invoke(candidate));

        _hub!.On<UserInfo>(HubRoutes.UserStatusChanged, user =>
            OnUserStatusChanged?.Invoke(user));
    }

    public Task RegisterAsync(string userId, string displayName) =>
        _hub!.InvokeAsync(HubRoutes.Register, userId, displayName);

    public Task<CallResponse> InitiateCallAsync(string calleeId) =>
        _hub!.InvokeAsync<CallResponse>(HubRoutes.InitiateCall, calleeId);

    public Task RespondToCallAsync(string callerId, bool accepted) =>
        _hub!.InvokeAsync(HubRoutes.RespondToCall, callerId, accepted);

    public Task EndCallAsync(string remoteUserId) =>
        _hub!.InvokeAsync(HubRoutes.EndCall, remoteUserId);

    public Task SendOfferAsync(string targetUserId, SessionDescription offer) =>
        _hub!.InvokeAsync(HubRoutes.SendOffer, targetUserId, offer);

    public Task SendAnswerAsync(string targetUserId, SessionDescription answer) =>
        _hub!.InvokeAsync(HubRoutes.SendAnswer, targetUserId, answer);

    public Task SendIceCandidateAsync(string targetUserId, IceCandidate candidate) =>
        _hub!.InvokeAsync(HubRoutes.SendIceCandidate, targetUserId, candidate);

    public Task<List<UserInfo>> GetOnlineUsersAsync() =>
        _hub!.InvokeAsync<List<UserInfo>>(HubRoutes.GetOnlineUsers);

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
        {
            await _hub.DisposeAsync();
            _hub = null;
        }
    }
}
