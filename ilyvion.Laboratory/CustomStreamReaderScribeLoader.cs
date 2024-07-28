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
            Logger.LogError("CustomStreamReaderScribeLoader is not properly initialized. " +
                "Is the 0ilyvion.Laboratory.dll library in use without the companion mod being active?");
            return;
        }
        initLoadingWithCustomStreamReader(Scribe.loader, loadStreamReader);
    }
}
