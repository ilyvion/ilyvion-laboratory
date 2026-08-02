using System.Diagnostics;

namespace ilyvion.Laboratory;

/// <summary>
/// Runtime on/off switch for the debug log categories passed to this library's internal
/// [Conditional("DEBUG")] logging (e.g. MultiTickCoroutines' "Coroutines" category). Any
/// category name can be enabled or disabled; the library does not restrict this to a fixed
/// set. Enabling a category only has an effect when ilyvion.Laboratory itself was built with
/// DEBUG defined, since the underlying log calls are compiled out of release builds.
/// </summary>
[SinceVersion(0, 23, 0)]
public static class IlyvionDebugLogCategories
{
    private static readonly HashSet<string> enabledCategories = [];

    public static void Enable(string category) => enabledCategories.Add(category);

    public static void Disable(string category) => enabledCategories.Remove(category);

    public static bool IsEnabled(string category) => enabledCategories.Contains(category);
}

internal static class Logger
{
    public static void LogMessage(string msg) => Log.Message($"[ilyvion's Laboratory] " + msg);

    public static void LogDevMessage(string msg)
    {
        if (Prefs.DevMode)
        {
            Log.Message($"[ilyvion's Laboratory][DEV] " + msg);
        }
    }

    [Conditional("DEBUG")]
    public static void LogDebug(string message, string? category = null)
    {
        if (category == null || IlyvionDebugLogCategories.IsEnabled(category))
        {
            LogDevMessage(message);
        }
    }

    public static void LogWarning(string msg) => Log.Warning($"[ilyvion's Laboratory] " + msg);

    public static void LogError(string msg) => Log.Error($"[ilyvion's Laboratory] " + msg);
}
