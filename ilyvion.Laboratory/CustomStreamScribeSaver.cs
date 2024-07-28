namespace ilyvion.Laboratory;

public static class CustomStreamScribeSaver
{
    internal static Action<ScribeSaver, Stream, string, bool>? initSavingWithCustomStream;

    /// <summary>
    /// Lets you do the equivalent of Scribe.saver.InitSaving() but with a custom
    /// Stream as the target instead of a file path.
    /// </summary>
    public static void InitSaving(Stream saveStream, string documentElementName, bool useIndentation = true)
    {
        if (initSavingWithCustomStream == null)
        {
            Logger.LogError("CustomStreamScribeSaver is not properly initialized. " +
                "Is the 0ilyvion.Laboratory.dll library in use without the companion mod being active?");
            return;
        }
        initSavingWithCustomStream(Scribe.saver, saveStream, documentElementName, useIndentation);
    }
}
