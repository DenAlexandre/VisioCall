namespace VisioCall.Maui.Models;

public class CallHistoryEntry
{
    public string UserId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public bool IsOutgoing { get; set; }
    public int DurationSeconds { get; set; }
}
