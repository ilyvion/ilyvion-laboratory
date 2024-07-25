using ilvyion.Laboratory;

namespace ilvyion.LaboratoryMod;

/// <summary>
/// Used to override the font with the custom font dictated by the CustomFontManager
/// </summary>
[HarmonyPatch(typeof(Text))]
[HarmonyPatch(nameof(Text.SpaceBetweenLines), MethodType.Getter)]
internal static class Verse_Text_SpaceBetweenLines
{
    private static bool Prefix(ref float __result)
    {
        float? currentSpaceBetweenLines = CustomFontManager.Instance.CurrentSpaceBetweenLines;
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
