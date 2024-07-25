namespace ilvyion.Laboratory;

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

    public static void LogWarning(string msg)
    {
        Log.Warning($"[ilyvion's Laboratory] " + msg);
    }

    public static void LogError(string msg)
    {
        Log.Error($"[ilyvion's Laboratory] " + msg);
    }
}
