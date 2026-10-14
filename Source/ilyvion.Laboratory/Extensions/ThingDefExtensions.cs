// Parts of the code:
// Copyright Karel Kroeze, 2020-2020

namespace ilyvion.Laboratory.Extensions;

[HotSwappable]
public static class ThingDefExtensions
{
    public static bool HasCompOrChildCompOf(this ThingDef def, Type compType)
    {
        ArgumentNullException.ThrowIfNull(def);
        ArgumentNullException.ThrowIfNull(compType);

        foreach (var compProperties in def.comps)
        {
            if (compType.IsAssignableFrom(compProperties.compClass))
            {
                return true;
            }
        }

        return false;
    }
}
