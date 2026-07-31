using System.Diagnostics;

namespace ilyvion.Laboratory;

internal static class Logger
{
    public static void LogMessage(string msg)
    {
        Log.Message($"[ilyvion's Laboratory] " + msg);
    }

    public static void LogDevMessage(string msg)
    {
        if (Prefs.DevMode)
        {
            Log.Message($"[ilyvion's Laboratory][DEV] " + msg);
        }
    }

    static readonly string[] enabledDebugLogCategories = [
        //"Coroutines"
    ];

    [Conditional("DEBUG")]
    public static void LogDebug(string message, string? category = null)
    {
        if (category == null || enabledDebugLogCategories.Contains(category))
        {
            LogDevMessage(message);
        }
    }

    public static void LogWarning(string msg)
    {
        Log.Warning($"[ilyvion's Laboratory] " + msg);
    }

    public static void LogError(string msg)
    {
        Log.Error($"[ilyvion's Laboratory] " + msg);
    }
}
