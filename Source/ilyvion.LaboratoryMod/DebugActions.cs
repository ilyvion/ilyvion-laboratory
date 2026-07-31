using ilyvion.Laboratory;
#if !v1_3 && !v1_4
using LudeonTK;
#endif

namespace ilyvion.LaboratoryMod;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class DebugActions
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    [IlyvionDebugAction(
        IlyvionDebugActionAttribute.IlyvionLaboratoryCategory,
        "Hot reload languages",
        displayPriority: 9999,
        allowedGameStates = AllowedGameStates.Entry
    )]
    private static void HotReloadLanguages() =>
        LongEventHandler.ExecuteWhenFinished(
            delegate
            {
                LanguageDatabase.Clear();
                LanguageDatabase.InitAllMetadata();
                GenLabel.ClearCache();
            }
        );

    [IlyvionDebugAction(
        IlyvionDebugActionAttribute.IlyvionLaboratoryCategory,
        "Hot reload languages",
        displayPriority: 9999,
        allowedGameStates = AllowedGameStates.Playing
    )]
#pragma warning disable IDE0051 // Used by reflection
    private static void HotReloadLanguagesPlaying() => HotReloadLanguages();
#pragma warning restore IDE0051
}
