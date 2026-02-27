namespace VisioCall.Shared.Constants;

public static class HubRoutes
{
    public const string CallHub = "/hubs/call";

    // Server methods (client -> server)
    public const string Register = nameof(Register);
    public const string InitiateCall = nameof(InitiateCall);
    public const string RespondToCall = nameof(RespondToCall);
    public const string EndCall = nameof(EndCall);
    public const string SendOffer = nameof(SendOffer);
    public const string SendAnswer = nameof(SendAnswer);
    public const string SendIceCandidate = nameof(SendIceCandidate);
    public const string GetOnlineUsers = nameof(GetOnlineUsers);

    // Client methods (server -> client)
    public const string IncomingCall = nameof(IncomingCall);
    public const string CallResponseReceived = nameof(CallResponseReceived);
    public const string CallEnded = nameof(CallEnded);
    public const string ReceiveOffer = nameof(ReceiveOffer);
    public const string ReceiveAnswer = nameof(ReceiveAnswer);
    public const string ReceiveIceCandidate = nameof(ReceiveIceCandidate);
    public const string UserStatusChanged = nameof(UserStatusChanged);
}
