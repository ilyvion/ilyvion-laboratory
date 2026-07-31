using System.Diagnostics;

namespace ilyvion.Laboratory;

public static class IlyvionDebugViewSettings
{
    internal static bool _shouldDrawUIHelpers;

    [Obsolete("Switch to ShouldDrawUIHelpers")]
    public static bool DrawUIHelpers => _shouldDrawUIHelpers;

    public static bool ShouldDrawUIHelpers => _shouldDrawUIHelpers;

    [Conditional("DEBUG")]
    public static void DrawIfUIHelpers(Action drawAction)
    {
        if (drawAction == null)
        {
            return;
        }

        if (_shouldDrawUIHelpers)
        {
            drawAction();
        }
    }
}
