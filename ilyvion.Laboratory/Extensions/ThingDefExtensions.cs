// Parts of the code:
// Copyright Karel Kroeze, 2020-2020

namespace ilyvion.Laboratory.Extensions;

[HotSwappable]
public static class ThingDefExtensions
{
    public static bool HasCompOrChildCompOf(this ThingDef def, Type compType)
    {
        if (def == null)
        {
            throw new ArgumentNullException(nameof(def));
        }
        if (compType == null)
        {
            throw new ArgumentNullException(nameof(compType));
        }

        foreach (CompProperties compProperties in def.comps)
        {
            if (compType.IsAssignableFrom(compProperties.compClass))
            {
                return true;
            }
        }

        return false;
    }
}
