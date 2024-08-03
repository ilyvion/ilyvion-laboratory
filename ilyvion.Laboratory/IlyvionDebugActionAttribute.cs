#if !v1_3 && !v1_4
using LudeonTK;
#endif

namespace ilyvion.Laboratory;

[AttributeUsage(AttributeTargets.Method)]
#pragma warning disable CA1813
public class IlyvionDebugActionAttribute : DebugActionAttribute
#pragma warning restore CA1813
{
#pragma warning disable CA1019
    public IlyvionDebugActionAttribute(string? category = null, string? name = null, bool requiresRoyalty = false, bool requiresIdeology = false, bool requiresBiotech = false, bool requiresAnomaly = false, int displayPriority = 0, bool hideInSubMenu = false)
#pragma warning restore CA1019
#if v1_3
    : base(category, name, requiresRoyalty, requiresIdeology)
#elif v1_4
    : base(category, name, requiresRoyalty, requiresIdeology, requiresBiotech, displayPriority, hideInSubMenu)
#else
    : base(category, name, requiresRoyalty, requiresIdeology, requiresBiotech, requiresAnomaly, displayPriority, hideInSubMenu)
#endif
    {
    }
}
