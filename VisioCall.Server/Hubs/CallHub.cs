using Microsoft.AspNetCore.SignalR;
using VisioCall.Server.Services;
using VisioCall.Shared.Constants;
using VisioCall.Shared.Models;

namespace VisioCall.Server.Hubs;

public class CallHub(UserTrackingService userTracking, ILogger<CallHub> logger) : Hub
{
    public async Task Register(string userId, string displayName)
    {
        userTracking.RegisterUser(userId, displayName, Context.ConnectionId);
        logger.LogInformation("User {UserId} ({DisplayName}) registered with connection {ConnectionId}",
            userId, displayName, Context.ConnectionId);

        // Notify all clients of the new user
        await Clients.Others.SendAsync(HubRoutes.UserStatusChanged,
            new UserInfo(userId, displayName, true));
    }

    public async Task<CallResponse> InitiateCall(string calleeId)
    {
        var callerId = userTracking.GetUserId(Context.ConnectionId);
        if (callerId is null)
            return new CallResponse(false, "Caller not registered");

        var calleeConnectionId = userTracking.GetConnectionId(calleeId);
        if (calleeConnectionId is null)
            return new CallResponse(false, "User is offline");

        var callerUsers = userTracking.GetOnlineUsers();
        var callerInfo = callerUsers.FirstOrDefault(u => u.UserId == callerId);
        var callerName = callerInfo?.DisplayName ?? callerId;

        logger.LogInformation("Call initiated: {Caller} -> {Callee}", callerId, calleeId);

        await Clients.Client(calleeConnectionId).SendAsync(HubRoutes.IncomingCall,
            new CallRequest(callerId, callerName, calleeId));

        return new CallResponse(true);
    }

    public async Task RespondToCall(string callerId, bool accepted)
    {
        var calleeId = userTracking.GetUserId(Context.ConnectionId);
        var callerConnectionId = userTracking.GetConnectionId(callerId);

        if (callerConnectionId is null || calleeId is null) return;

        logger.LogInformation("Call response from {Callee} to {Caller}: {Accepted}",
            calleeId, callerId, accepted);

        await Clients.Client(callerConnectionId).SendAsync(HubRoutes.CallResponseReceived, accepted);
    }

    public async Task EndCall(string remoteUserId)
    {
        var remoteConnectionId = userTracking.GetConnectionId(remoteUserId);
        if (remoteConnectionId is null) return;

        var userId = userTracking.GetUserId(Context.ConnectionId);
        logger.LogInformation("Call ended by {UserId} with {RemoteUserId}", userId, remoteUserId);

        await Clients.Client(remoteConnectionId).SendAsync(HubRoutes.CallEnded);
    }

    public async Task SendOffer(string targetUserId, SessionDescription offer)
    {
        var targetConnectionId = userTracking.GetConnectionId(targetUserId);
        if (targetConnectionId is null) return;

        await Clients.Client(targetConnectionId).SendAsync(HubRoutes.ReceiveOffer, offer);
    }

    public async Task SendAnswer(string targetUserId, SessionDescription answer)
    {
        var targetConnectionId = userTracking.GetConnectionId(targetUserId);
        if (targetConnectionId is null) return;

        await Clients.Client(targetConnectionId).SendAsync(HubRoutes.ReceiveAnswer, answer);
    }

    public async Task SendIceCandidate(string targetUserId, IceCandidate candidate)
    {
        var targetConnectionId = userTracking.GetConnectionId(targetUserId);
        if (targetConnectionId is null) return;

        await Clients.Client(targetConnectionId).SendAsync(HubRoutes.ReceiveIceCandidate, candidate);
    }

    public List<UserInfo> GetOnlineUsers() => userTracking.GetOnlineUsers();

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = userTracking.GetUserId(Context.ConnectionId);
        userTracking.RemoveByConnectionId(Context.ConnectionId);

        if (userId is not null)
        {
            logger.LogInformation("User {UserId} disconnected", userId);
            await Clients.Others.SendAsync(HubRoutes.UserStatusChanged,
                new UserInfo(userId, userId, false));
        }

        await base.OnDisconnectedAsync(exception);
    }
}
