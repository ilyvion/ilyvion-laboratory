#if v1_5
using LudeonTK;
#endif
using RimWorld;

namespace ilyvion.LaboratoryMod;

public static class DebugActions
{
    [DebugAction(
        "ilyvion",
        "Hot reload languages",
        false,
        false,
#if !v1_3
        false,
#endif
#if !v1_4 && !v1_3
        false,
#endif
#if !v1_3
        9999,
        false,
#endif
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

    [DebugAction(
        "ilyvion",
        "Hot reload languages",
        false,
        false,
#if !v1_3
        false,
#endif
#if !v1_4 && !v1_3
        false,
#endif
#if !v1_3
        9999,
        false,
#endif
        allowedGameStates = AllowedGameStates.Playing
    )]
    private static void HotReloadLanguagesPlaying()
    {
        HotReloadLanguages();
    }
}
