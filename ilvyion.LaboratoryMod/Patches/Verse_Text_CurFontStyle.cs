using ilvyion.Laboratory;

namespace ilvyion.LaboratoryMod;

/// <summary>
/// Used to override the font with the custom font dictated by the CustomFontManager
/// </summary>
[HarmonyPatch(typeof(Text))]
[HarmonyPatch(nameof(Text.CurFontStyle), MethodType.Getter)]
internal static class Verse_Text_CurFontStyle
{
    private static bool Prefix(ref GUIStyle __result)
    {
        GUIStyle? currentFontStyle = CustomFontManager.Instance.CurrentFontStyle;
        if (currentFontStyle != null)
        {
            currentFontStyle.alignment = Text.Anchor;
            currentFontStyle.wordWrap = Text.WordWrap;

            __result = currentFontStyle;
            return false;
        }
        else
        {
            return true;
        }
    }
}
