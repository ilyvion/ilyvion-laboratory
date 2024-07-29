using LudeonTK;
using RimWorld;

namespace ilyvion.LaboratoryMod;

public static class DebugActions
{
    [DebugAction("ilyvion", "Hot reload languages", false, false, false, false, 0, false,
        allowedGameStates = AllowedGameStates.Entry,
        displayPriority = 9999)]
    private static void HotReloadLanguages()
    {
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            LanguageDatabase.Clear();
            LanguageDatabase.InitAllMetadata();
            GenLabel.ClearCache();
        });
    }

    [DebugAction("ilyvion", "Hot reload languages", false, false, false, false, 0, false,
        allowedGameStates = AllowedGameStates.Playing,
        displayPriority = 9999)]
    private static void HotReloadLanguagesPlaying()
    {
        HotReloadLanguages();
    }
}
