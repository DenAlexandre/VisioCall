using System.Text.Json;
using VisioCall.Maui.Models;

namespace VisioCall.Maui.Services;

public class CallHistoryService
{
    private const string StorageKey = "CallHistory";
    private const int MaxEntries = 50;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private List<CallHistoryEntry>? _cache;

    public List<CallHistoryEntry> GetHistory()
    {
        _cache ??= Load();
        return _cache;
    }

    public void Add(CallHistoryEntry entry)
    {
        var history = GetHistory();
        history.Insert(0, entry);

        if (history.Count > MaxEntries)
            history.RemoveRange(MaxEntries, history.Count - MaxEntries);

        Save(history);
    }

    private static List<CallHistoryEntry> Load()
    {
        var json = Preferences.Get(StorageKey, "");
        if (string.IsNullOrEmpty(json)) return [];

        try
        {
            return JsonSerializer.Deserialize<List<CallHistoryEntry>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static void Save(List<CallHistoryEntry> history)
    {
        var json = JsonSerializer.Serialize(history, JsonOptions);
        Preferences.Set(StorageKey, json);
    }
}
