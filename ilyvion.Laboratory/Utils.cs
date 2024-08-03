namespace ilyvion.Laboratory;

internal static class Utils
{
    public static void LogMissingInitialization(string problematicType)
    {
        Logger.LogError($"{problematicType} is not properly initialized. " +
            $"Is the {Constants.AssemblyName} library in use without the companion mod being active?");
    }
}
