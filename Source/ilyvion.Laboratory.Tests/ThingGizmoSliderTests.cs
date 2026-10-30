// ThingGizmoSliderTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

#if v1_5_OR_GREATER
using DevTools.Testing;
using ilyvion.Laboratory.UI;

namespace ilyvion.Laboratory.Tests;

// The hash code must not change between the per-frame instances of a gizmo, or vanilla never
// shows its tooltip.
[TestFixture(TestType.MainMenu)]
internal sealed class ThingGizmoSliderTests
{
    private sealed class FirstSlider(Thing owner) : ThingGizmoSlider(owner)
    {
        protected override float Target { get; set; }
        protected override float ValuePercent => 0f;
        protected override string Title => "First";
        protected override bool IsDraggable => false;
        protected override string BarLabel => "First";
        protected override bool DraggingBar { get; set; }

        protected override string GetTooltip() => "First";
    }

    private sealed class SecondSlider(Thing owner) : ThingGizmoSlider(owner)
    {
        protected override float Target { get; set; }
        protected override float ValuePercent => 0f;
        protected override string Title => "Second";
        protected override bool IsDraggable => false;
        protected override string BarLabel => "Second";
        protected override bool DraggingBar { get; set; }

        protected override string GetTooltip() => "Second";
    }

    private static Thing MakeThing(int thingIDNumber)
    {
        var thing = ActivatorUtility.CreateInstance<Thing>(typeof(Thing));
        thing.thingIDNumber = thingIDNumber;
        return thing;
    }

    [Test]
    public static void SameGizmoTypeOnSameThingHasSameHash()
    {
        var thing = MakeThing(1001);

        Expect.AreEqual(new FirstSlider(thing).GetHashCode(), new FirstSlider(thing).GetHashCode());
    }

    [Test]
    public static void SameGizmoTypeOnDifferentThingsHasDifferentHashes() =>
        Expect.AreNotEqual(
            new FirstSlider(MakeThing(1001)).GetHashCode(),
            new FirstSlider(MakeThing(1002)).GetHashCode()
        );

    [Test]
    public static void DifferentGizmoTypesOnSameThingHaveDifferentHashes()
    {
        var thing = MakeThing(1001);

        Expect.AreNotEqual(
            new FirstSlider(thing).GetHashCode(),
            new SecondSlider(thing).GetHashCode()
        );
    }
}
#endif
