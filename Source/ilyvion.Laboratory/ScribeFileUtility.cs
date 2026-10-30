// ScribeFileUtility.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

namespace ilyvion.Laboratory;

/// <summary>
/// Helpers for saving data to, and loading it from, an arbitrary file with Scribe, outside of
/// the game's own save and load flow.
/// </summary>
[SinceVersion(0, 27, 0)]
public static class ScribeFileUtility
{
    /// <summary>
    /// Writes the file at <paramref name="path"/> with a root node named
    /// <paramref name="rootNodeName"/>, a meta header and whatever <paramref name="look"/> saves.
    /// The file is finalized even when <paramref name="look"/> throws, and the exception then
    /// propagates.
    /// </summary>
    public static void Save(string path, string rootNodeName, Action look)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(rootNodeName);
        ArgumentNullException.ThrowIfNull(look);

        Scribe.saver.InitSaving(path, rootNodeName);
        try
        {
            ScribeMetaHeaderUtility.WriteMetaHeader();
            look();
        }
        finally
        {
            Scribe.saver.FinalizeSaving();
        }
    }

    /// <summary>
    /// Reads the file at <paramref name="path"/>, skipping the meta header's checks, and runs
    /// whatever <paramref name="look"/> loads. When reading fails, Scribe is stopped with
    /// <see cref="Scribe.ForceStop"/> and the exception propagates.
    /// </summary>
    public static void Load(string path, Action look)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(look);

        Scribe.loader.InitLoading(path);
        try
        {
            ScribeMetaHeaderUtility.LoadGameDataHeader(
                ScribeMetaHeaderUtility.ScribeHeaderMode.None,
                logVersionConflictWarning: true
            );
            look();
            Scribe.loader.FinalizeLoading();
        }
        catch
        {
            Scribe.ForceStop();
            throw;
        }
    }
}
