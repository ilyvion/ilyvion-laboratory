using System.Diagnostics;

[assembly: InternalsVisibleTo("ilyvion.LaboratoryMod")]
[assembly: InternalsVisibleTo("ilyvion.Laboratory.Tests")]

namespace ilyvion.Laboratory;

public abstract class IlyvionMod(ModContentPack content) : Mod(content)
{
    protected virtual bool HasSettings => false;

    public override string SettingsCategory() => HasSettings ? Content.Name : "";

    /// <summary>
    /// Applies every Harmony patch in the assembly that declares this mod class.
    /// </summary>
    /// <param name="harmonyId">The Harmony id to patch with; defaults to the mod's package id.</param>
    /// <returns>The <see cref="Harmony"/> instance that applied the patches.</returns>
    [SinceVersion(0, 27, 0)]
    protected Harmony PatchAll(string? harmonyId = null)
    {
        var harmony = new Harmony(harmonyId ?? Content.PackageId);
        harmony.PatchAll(GetType().Assembly);
        return harmony;
    }

    /// <summary>
    /// Loads the mod's settings once the game has finished its current long event. Settings read
    /// for the first time while a save is loading fail, because the Scribe can only load one
    /// thing at a time.
    /// </summary>
    /// <typeparam name="T">The mod's settings class.</typeparam>
    [SinceVersion(0, 27, 0)]
    protected void LoadSettingsWhenFinished<T>()
        where T : ModSettings, new() =>
        LongEventHandler.ExecuteWhenFinished(() => GetSettings<T>());

    public virtual void LogMessage(string msg) => Log.Message($"[{Content.Name}] " + msg);

    public virtual void LogDevMessage(string msg) => LogDevMessage(() => msg);

    [SinceVersion(0, 20, 0)]
    public virtual void LogDevMessage(Func<string> produceMsg)
    {
        if (Prefs.DevMode)
        {
            ArgumentNullException.ThrowIfNull(produceMsg);

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

    public virtual void LogException(string msg, Exception e) => LogError($"{msg}\n{e}");

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
