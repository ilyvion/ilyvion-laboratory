namespace ilyvion.Laboratory;

public static class CustomStreamReaderScribeLoader
{
    internal static Action<ScribeLoader, StreamReader>? initLoadingWithCustomStreamReader;

    /// <summary>
    /// Lets you do the equivalent of Scribe.loader.InitLoading() but with a custom
    /// StreamReader as the source instead of a file path.
    /// </summary>
    public static void InitLoading(StreamReader loadStreamReader)
    {
        if (initLoadingWithCustomStreamReader == null)
        {
            Utils.LogMissingInitialization(nameof(CustomStreamReaderScribeLoader));
            return;
        }
        initLoadingWithCustomStreamReader(Scribe.loader, loadStreamReader);
    }
}
