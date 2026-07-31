// Verse_BackCompatibility_GetBackCompatibleType.cs
// Copyright (c) 2024 Alexander Krivács Schrøder

using System.Xml;
using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.GetBackCompatibleType))]
internal static class Verse_BackCompatibility_GetBackCompatibleType
{
    private static bool Prefix(
        Type baseType,
        string providedClassName,
        XmlNode node,
        ref Type __result)
    {
        if (CustomBackCompatibility.TypeReplacements.Count > 0
            && CustomBackCompatibility.TypeReplacements.TryGetValue(baseType, out var newType))
        {
            __result = newType;
            return false;
        }
        if (CustomBackCompatibility.ProvidedClassNameReplacements.Count > 0
            && CustomBackCompatibility.ProvidedClassNameReplacements.TryGetValue(
                providedClassName,
                out newType))
        {
            __result = newType;
            return false;
        }
        foreach (var customReplacement in CustomBackCompatibility.CustomReplacements)
        {
            if (customReplacement(baseType, providedClassName, node) is Type type)
            {
                __result = type;
                return false;
            }
        }
        return true;
    }
}
