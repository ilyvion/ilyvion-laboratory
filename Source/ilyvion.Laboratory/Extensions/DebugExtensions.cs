namespace ilyvion.Laboratory.Extensions;

public static class DebugExtensions
{
    public static T Dump<T>(this T dump)
    {
        Logger.LogDevMessage("DUMP: " + dump?.ToString() ?? "<null>");
        return dump;
    }
    public static T Dump<T, TDumped>(this T dump, Func<T, TDumped> dumper)
    {
        if (dumper == null)
        {
            throw new ArgumentNullException(nameof(dumper));
        }
        Logger.LogDevMessage("DUMP: " + dumper(dump)?.ToString() ?? "<null>");
        return dump;
    }
}
