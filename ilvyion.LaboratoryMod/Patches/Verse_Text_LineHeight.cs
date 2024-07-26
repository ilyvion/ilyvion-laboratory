using ilvyion.Laboratory;

namespace ilvyion.LaboratoryMod;

/// <summary>
/// Used to override the font with the custom font dictated by the CustomFontManager
/// </summary>
[HarmonyPatch(typeof(Text))]
[HarmonyPatch(nameof(Text.LineHeight), MethodType.Getter)]
[HarmonyPatchCategory("Late")]
internal static class Verse_Text_LineHeight
{
    private static bool Prepare()
    {
        return CustomFontManager.featureEnabled;
    }

    private static bool Prefix(ref float __result)
    {
        float? currentLineHeight = CustomFontManager.Instance.CurrentLineHeight;
        if (currentLineHeight.HasValue)
        {
            __result = currentLineHeight.Value;
            return false;
        }
        else
        {
            return true;
        }
    }
}
