using System.Diagnostics;

[assembly: InternalsVisibleTo("ilyvion.LaboratoryMod")]

namespace ilyvion.Laboratory;

public abstract class IlyvionMod(ModContentPack content) : Mod(content)
{
    protected virtual bool HasSettings => false;

    public override string SettingsCategory() => HasSettings ? Content.Name : "";

    public virtual void LogMessage(string msg) => Log.Message($"[{Content.Name}] " + msg);

    public virtual void LogDevMessage(string msg) => LogDevMessage(() => msg);

    [SinceVersion(0, 20, 0)]
    public virtual void LogDevMessage(Func<string> produceMsg)
    {
        if (Prefs.DevMode)
        {
            if (produceMsg == null)
            {
                throw new ArgumentNullException(nameof(produceMsg));
            }

            Log.Message($"[{Content.Name}][DEV] " + produceMsg());
        }
    }

    [Conditional("DEBUG")]
    public virtual void LogDebug(string message) => LogDevMessage(message);

    [Conditional("DEBUG")]
    [SinceVersion(0, 20, 0)]
    public virtual void LogDebug(Func<string> produceMsg) => LogDevMessage(produceMsg);

    public virtual void LogWarning(string msg) => Log.Warning($"[{Content.Name}] " + msg);

    public virtual void LogError(string msg) => Log.Error($"[{Content.Name}] " + msg);

    public virtual void LogException(string msg, Exception e) =>
        Log.Error(
            $"""
                {msg}
                {e}
            """
        );

    public virtual void LogMessageOnce(string msg, ref bool hasLogged)
    {
        if (!hasLogged)
        {
            LogMessage(msg);
            hasLogged = true;
        }
    }

    public virtual void LogDevMessageOnce(string msg, ref bool hasLogged)
    {
        if (!hasLogged)
        {
            LogDevMessage(msg);
            hasLogged = true;
        }
    }

    [Conditional("DEBUG")]
    public virtual void LogDebugOnce(string message, ref bool hasLogged)
    {
        if (!hasLogged)
        {
            LogDebug(message);
            hasLogged = true;
        }
    }

    public virtual void LogWarningOnce(string msg, ref bool hasLogged)
    {
        if (!hasLogged)
        {
            LogWarning(msg);
            hasLogged = true;
        }
    }

    public virtual void LogErrorOnce(string msg, ref bool hasLogged)
    {
        if (!hasLogged)
        {
            LogError(msg);
            hasLogged = true;
        }
    }

    public virtual void LogExceptionOnce(string msg, Exception e, ref bool hasLogged)
    {
        if (!hasLogged)
        {
            LogException(msg, e);
            hasLogged = true;
        }
    }
}
