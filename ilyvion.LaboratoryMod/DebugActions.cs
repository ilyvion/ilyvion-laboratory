using ilyvion.Laboratory;
#if !v1_3 && !v1_4
using LudeonTK;
#endif

namespace ilyvion.LaboratoryMod;

public static class DebugActions
{
    [IlyvionDebugAction(
        "ilyvion",
        "Hot reload languages",
        displayPriority: 9999,
        allowedGameStates = AllowedGameStates.Entry
    )]
    private static void HotReloadLanguages()
    {
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            LanguageDatabase.Clear();
            LanguageDatabase.InitAllMetadata();
            GenLabel.ClearCache();
        });
    }

    [IlyvionDebugAction(
        "ilyvion",
        "Hot reload languages",
        displayPriority: 9999,
        allowedGameStates = AllowedGameStates.Playing
    )]
    private static void HotReloadLanguagesPlaying()
    {
        HotReloadLanguages();
    }
}
