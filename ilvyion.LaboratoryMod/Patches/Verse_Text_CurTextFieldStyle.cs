using ilvyion.Laboratory;

namespace ilvyion.LaboratoryMod;

/// <summary>
/// Used to override the font with the custom font dictated by the CustomFontManager
/// </summary>
[HarmonyPatch(typeof(Text))]
[HarmonyPatch(nameof(Text.CurTextFieldStyle), MethodType.Getter)]
[HarmonyPatchCategory("Late")]
internal static class Verse_Text_CurTextFieldStyle
{
    private static bool Prepare()
    {
        return CustomFontManager.featureEnabled;
    }

    private static bool Prefix(ref GUIStyle __result)
    {
        GUIStyle? currentTextFieldStyle = CustomFontManager.Instance.CurrentTextFieldStyle;
        if (currentTextFieldStyle != null)
        {
            __result = currentTextFieldStyle;
            return false;
        }
        else
        {
            return true;
        }
    }
}
