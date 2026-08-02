// CustomFontManager relies on Prepare() re-evaluating after dependent mods have had a
// chance to call EnableFeature(); only v1.5+'s "Late" patch category defers patching that
// long, so the feature is unavailable on 1.3/1.4.
#if v1_5_OR_GREATER
using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

/// <summary>
/// Used to override the font with the custom font dictated by the CustomFontManager
/// </summary>
[HarmonyPatch(typeof(Text))]
[HarmonyPatch(nameof(Text.SpaceBetweenLines), MethodType.Getter)]
[HarmonyPatchCategory("Late")]
internal static class Verse_Text_SpaceBetweenLines
{
    private static bool Prepare() => CustomFontManager.featureEnabled;

    private static bool Prefix(ref float __result)
    {
        var currentSpaceBetweenLines = CustomFontManager.Instance.CurrentSpaceBetweenLines;
        if (currentSpaceBetweenLines.HasValue)
        {
            __result = currentSpaceBetweenLines.Value;
            return false;
        }
        else
        {
            return true;
        }
    }
}
#endif
