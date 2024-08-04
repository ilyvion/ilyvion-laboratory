using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ilyvion.LaboratoryMod")]

namespace ilyvion.Laboratory;

public abstract class IlyvionMod(ModContentPack content) : Mod(content)
{
    protected virtual bool HasSettings => false;
    public override string SettingsCategory()
    {
        return HasSettings ? Content.Name : "";
    }

    public virtual void LogMessage(string msg)
    {
        Log.Message($"[{Content.Name}] " + msg);
    }

    public virtual void LogDevMessage(string msg)
    {
        if (Prefs.DevMode)
        {
            Log.Message($"[{Content.Name}][DEV] " + msg);
        }
    }

    [Conditional("DEBUG")]
    public virtual void LogDebug(string message)
    {
        LogDevMessage(message);
    }

    public virtual void LogWarning(string msg)
    {
        Log.Warning($"[{Content.Name}] " + msg);
    }

    public virtual void LogError(string msg)
    {
        Log.Error($"[{Content.Name}] " + msg);
    }

    public virtual void LogException(string msg, Exception e)
    {
        Log.Error($"""
            {msg}
            {e}
        """);
    }
}
