namespace ilyvion.Laboratory;

public static class Utils
{
    internal static void LogMissingInitialization(string problematicType)
    {
        Logger.LogError($"{problematicType} is not properly initialized. " +
            $"Is the {Constants.AssemblyName} library in use without the companion mod being active?");
    }

    /// <summary>
    /// Round up to given precision
    /// </summary>
    /// <param name="value">input</param>
    /// <param name="precision">number of digits to preserve past the magnitude, should be equal to or greater than zero.</param>
    public static int CeilToPrecision(float value, int precision = 1)
    {
        var magnitude = Mathf.FloorToInt(Mathf.Log10(value + 1));
        var unit = Mathf.FloorToInt(Mathf.Pow(10, Mathf.Max(magnitude - precision, 1)));
        return Mathf.CeilToInt((value + 1) / unit) * unit;
    }

    public static string FormatCount(float value, string suffix, int threshold = 1000, string[]? unitSuffixes = null)
    {
        unitSuffixes ??= ["", "k", "M", "G"];

        var i = 0;
        while (value > threshold && i < unitSuffixes.Length)
        {
            value /= threshold;
            i++;
        }

        return value.ToString("0.# " + unitSuffixes[i] + suffix);
    }
}
