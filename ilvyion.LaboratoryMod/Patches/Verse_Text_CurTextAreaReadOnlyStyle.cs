using ilvyion.Laboratory;

namespace ilvyion.LaboratoryMod;

/// <summary>
/// Used to override the font with the custom font dictated by the CustomFontManager
/// </summary>
[HarmonyPatch(typeof(Text))]
[HarmonyPatch(nameof(Text.CurTextAreaReadOnlyStyle), MethodType.Getter)]
[HarmonyPatchCategory("Late")]
internal static class Verse_Text_CurTextAreaReadOnlyStyle
{
    private static bool Prepare()
    {
        return CustomFontManager.featureEnabled;
    }

    private static bool Prefix(ref GUIStyle __result)
    {
        GUIStyle? currentTextAreaReadOnlyStyle = CustomFontManager.Instance.CurrentTextAreaReadOnlyStyle;
        if (currentTextAreaReadOnlyStyle != null)
        {
            __result = currentTextAreaReadOnlyStyle;
            return false;
        }
        else
        {
            return true;
        }
    }
}
