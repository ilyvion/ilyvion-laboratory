// ThingGizmoSlider.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

#if v1_5_OR_GREATER
namespace ilyvion.Laboratory.UI;

/// <summary>
/// A <see cref="Gizmo_Slider"/> for a gizmo that is created anew every GUI frame to control one
/// <see cref="Thing"/>. Vanilla only shows a gizmo's tooltip once the same hash code has been
/// hovered for a while, so a gizmo whose hash code changes every frame never shows one; this
/// class's hash code is derived from the concrete gizmo type and the owning thing instead.
/// </summary>
[SinceVersion(0, 27, 0)]
public abstract class ThingGizmoSlider : Gizmo_Slider
{
    private readonly Thing _owner;

    /// <summary>Initializes a new instance controlling <paramref name="owner"/>.</summary>
    /// <param name="owner">The thing the gizmo belongs to.</param>
    protected ThingGizmoSlider(Thing owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        _owner = owner;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (GetType().GetHashCode() * 397) ^ _owner.thingIDNumber;
        }
    }
}
#endif
