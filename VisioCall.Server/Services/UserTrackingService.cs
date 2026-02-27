using System.Collections.Concurrent;
using VisioCall.Shared.Models;

namespace VisioCall.Server.Services;

public class UserTrackingService
{
    // userId -> connectionId
    private readonly ConcurrentDictionary<string, string> _userConnections = new();
    // userId -> displayName
    private readonly ConcurrentDictionary<string, string> _userNames = new();

    public bool RegisterUser(string userId, string displayName, string connectionId)
    {
        _userNames[userId] = displayName;
        _userConnections[userId] = connectionId;
        return true;
    }

    public void RemoveByConnectionId(string connectionId)
    {
        var entry = _userConnections.FirstOrDefault(x => x.Value == connectionId);
        if (entry.Key is not null)
        {
            _userConnections.TryRemove(entry.Key, out _);
        }
    }

    public string? GetConnectionId(string userId) =>
        _userConnections.TryGetValue(userId, out var connId) ? connId : null;

    public string? GetUserId(string connectionId) =>
        _userConnections.FirstOrDefault(x => x.Value == connectionId).Key;

    public List<UserInfo> GetOnlineUsers() =>
        _userConnections.Select(kvp => new UserInfo(
            kvp.Key,
            _userNames.TryGetValue(kvp.Key, out var name) ? name : kvp.Key,
            true
        )).ToList();
}
