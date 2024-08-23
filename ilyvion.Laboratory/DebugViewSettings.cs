using System.Diagnostics;

namespace ilyvion.Laboratory;

public static class IlyvionDebugViewSettings
{
    internal static bool shouldDrawUIHelpers;

    [Obsolete("Switch to ShouldDrawUIHelpers")]
    public static bool DrawUIHelpers => shouldDrawUIHelpers;

    public static bool ShouldDrawUIHelpers => shouldDrawUIHelpers;

    [Conditional("DEBUG")]
    public static void DrawIfUIHelpers(Action drawAction)
    {
        if (drawAction == null)
        {
            return;
        }

        if (shouldDrawUIHelpers)
        {
            drawAction();
        }
    }
}
