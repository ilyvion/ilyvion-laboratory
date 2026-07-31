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
            Utils.LogMissingInitialization(nameof(CustomStreamScribeSaver));
            return;
        }
        initSavingWithCustomStream(Scribe.saver, saveStream, documentElementName, useIndentation);
    }
}
