using ilvyion.Laboratory;

namespace ilvyion.LaboratoryMod;

/// <summary>
/// Used to override the font with the custom font dictated by the CustomFontManager
/// </summary>
[HarmonyPatch(typeof(Text))]
[HarmonyPatch(nameof(Text.CurTextAreaStyle), MethodType.Getter)]
internal static class Verse_Text_CurTextAreaStyle
{
    private static bool Prefix(ref GUIStyle __result)
    {
        GUIStyle? currentTextAreaStyle = CustomFontManager.Instance.CurrentTextAreaStyle;
        if (currentTextAreaStyle != null)
        {
            __result = currentTextAreaStyle;
            return false;
        }
        else
        {
            return true;
        }
    }
}
