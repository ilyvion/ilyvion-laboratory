// ScribeEitherTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;
using RimWorld.Planet;

namespace ilyvion.Laboratory.Tests;

// Regression suite for commit 0bf0f96 ("fix(either): address scribe data loss, BodyPart null
// handling and incorrect docs"). Before that fix, Scribe_Either.LookValue called the various
// Scribe_X.Look(ref localVar, ...) helpers for the Reference/LocalTargetInfo/TargetInfo/
// GlobalTargetInfo/BodyPart lookmodes but never wrote localVar back into the `value` parameter
// (itself a `ref either._left`/`ref either._right`), so anything loaded through those lookmodes
// was silently dropped: the Either kept whatever default it started with instead of the value
// Scribe just resolved. These tests drive a real Scribe save/load/cross-ref-resolve cycle (via
// ilyvion.Laboratory's CustomStream Scribe helpers, so no on-disk save file is needed) and assert
// the round-tripped Either actually carries the resolved value.
[TestSuite]
internal static class ScribeEitherTests
{
    /// <summary>
    /// A <see cref="Stream"/> wrapper whose dispose is a no-op, so the wrapped stream survives
    /// Scribe's own save/load lifecycle (which closes whatever stream it was handed) and can
    /// still be read from/written to afterwards.
    /// </summary>
    private sealed class NonClosingStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush() => inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) =>
            inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        public override void SetLength(long value) => inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) =>
            inner.Write(buffer, offset, count);
    }

    /// <summary>
    /// A minimal cross-referenceable object usable with <see cref="LookMode.Reference"/> without
    /// needing a real <see cref="Thing"/>/map.
    /// </summary>
    private sealed class FakeReferenceable : IExposable, ILoadReferenceable
    {
        public string LoadId = Guid.NewGuid().ToString();

        public string GetUniqueLoadID() => LoadId;

        public void ExposeData() { }
    }

    private static int _nextThingId = 1;

    /// <summary>
    /// A <see cref="Thing"/> stand-in with a no-op <see cref="ExposeData"/>, used as a
    /// <see cref="LocalTargetInfo"/>/<see cref="TargetInfo"/>/<see cref="GlobalTargetInfo"/>
    /// target without needing a real <see cref="Thing"/>/map. Two things need bypassing to make a
    /// Thing usable outside a loaded Game: <see cref="Thing.PostMake"/> calls
    /// <see cref="ThingIDMaker.GiveIDTo(Thing)"/>, which needs <see cref="Find.UniqueIDsManager"/>
    /// (worked around by never calling <see cref="ThingMaker.MakeThing(ThingDef, ThingDef)"/>, and
    /// setting <see cref="Thing.thingIDNumber"/> directly instead); and the real
    /// <see cref="Thing.ExposeData"/> touches further game state (e.g.
    /// <see cref="Find.FactionManager"/>) when Scribe re-invokes it during
    /// <see cref="CrossRefHandler.ResolveAllCrossReferences"/> (worked around by this no-op
    /// override — its own field data isn't what these tests are exercising).
    /// </summary>
    private sealed class FakeThing : Thing
    {
        public override void ExposeData() { }
    }

    private static FakeThing MakeBareThing(ThingDef def) =>
        new() { def = def, thingIDNumber = _nextThingId++ };

    /// <summary>
    /// Drives <see cref="Scribe_Either.Look{TLeft, TRight}"/> through a full save, then a
    /// load/resolve-cross-refs/post-load-init cycle, entirely in memory. Mirrors the standard
    /// IExposable pattern of registering itself (and any referenced objects) for cross-ref
    /// resolution while loading, since that's the only way lookmodes like
    /// <see cref="LookMode.Reference"/> get a second pass to resolve their reference.
    /// </summary>
    private sealed class ScribeRoundTripHarness<TLeft, TRight>(
        string label,
        LookMode leftLookMode,
        LookMode rightLookMode,
        IReadOnlyList<ILoadReferenceable> extraCrossRefs
    ) : IExposable
    {
        public Either<TLeft, TRight>? Value;

        public void ExposeData()
        {
#pragma warning disable IDE0001 // Simplify Names
            Scribe_Either.Look<TLeft, TRight>(
                ref Value,
                label,
                leftLookMode: leftLookMode,
                rightLookMode: rightLookMode
            );
#pragma warning restore IDE0001 // Simplify Names

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                // Extra cross-refs are never actually deep-saved anywhere in this harness's
                // document (they're kept alive in test memory across the round trip instead), so
                // tell the DevMode diagnostic checker not to warn about that.
                foreach (var extraCrossRef in extraCrossRefs)
                {
                    Scribe.saver.loadIDsErrorsChecker.RegisterDeepSaved(extraCrossRef, label);
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe.loader.crossRefs.RegisterForCrossRefResolve(this);
                foreach (var extraCrossRef in extraCrossRefs)
                {
                    Scribe.loader.crossRefs.RegisterForCrossRefResolve((IExposable)extraCrossRef);
                }
            }
        }

        public static Either<TLeft, TRight>? RoundTrip(
            Either<TLeft, TRight>? value,
            LookMode leftLookMode = LookMode.Undefined,
            LookMode rightLookMode = LookMode.Undefined,
            params ILoadReferenceable[] extraCrossRefs
        )
        {
            const string label = "either";

            using var memory = new MemoryStream();
            using (var nonClosing = new NonClosingStream(memory))
            {
                CustomStreamScribeSaver.InitSaving(nonClosing, "root");
                try
                {
                    var saveHarness = new ScribeRoundTripHarness<TLeft, TRight>(
                        label,
                        leftLookMode,
                        rightLookMode,
                        extraCrossRefs
                    )
                    {
                        Value = value,
                    };
                    saveHarness.ExposeData();
                    Scribe.saver.FinalizeSaving();
                }
                finally
                {
                    if (Scribe.mode != LoadSaveMode.Inactive)
                    {
                        Scribe.ForceStop();
                    }
                }
            }

            memory.Position = 0;
            var loadHarness = new ScribeRoundTripHarness<TLeft, TRight>(
                label,
                leftLookMode,
                rightLookMode,
                extraCrossRefs
            );
            try
            {
                using (var reader = new StreamReader(memory))
                {
                    CustomStreamReaderScribeLoader.InitLoading(reader);
                }
                // CrossRefHandler.ResolveAllCrossReferences() sets Scribe.loader.curParent to this
                // harness before re-invoking ExposeData() during ResolvingCrossRefs, so curParent
                // must match here too during LoadingVars, or RegisterLoadIDReadFromXml/Take<T>'s
                // (parent, pathRelToParent) keys won't line up between the two passes.
                Scribe.loader.curParent = loadHarness;
                loadHarness.ExposeData();
                Scribe.loader.FinalizeLoading();
            }
            finally
            {
                if (Scribe.mode != LoadSaveMode.Inactive)
                {
                    Scribe.ForceStop();
                }
            }

            return loadHarness.Value;
        }
    }

    [Test]
    public static void ReferenceLookModeRoundTripsLeftValue()
    {
        // Regression guard: previously the resolved reference was computed but never written
        // back into `either._left`, so the loaded Either kept its default (null) left value.
        var referenced = new FakeReferenceable();
        var original = Either<FakeReferenceable, FakeReferenceable>.Left(referenced);

        var loaded = ScribeRoundTripHarness<FakeReferenceable, FakeReferenceable>.RoundTrip(
            original,
            leftLookMode: LookMode.Reference,
            rightLookMode: LookMode.Reference,
            referenced
        );

        Assert.That(loaded != null).Is.True();
        Assert.That(loaded!.IsLeft).Is.True();
        Assert.That(ReferenceEquals(loaded.UnwrapLeft(), referenced)).Is.True();
    }

    [Test]
    public static void ReferenceLookModeRoundTripsRightValue()
    {
        var referenced = new FakeReferenceable();
        var original = Either<FakeReferenceable, FakeReferenceable>.Right(referenced);

        var loaded = ScribeRoundTripHarness<FakeReferenceable, FakeReferenceable>.RoundTrip(
            original,
            leftLookMode: LookMode.Reference,
            rightLookMode: LookMode.Reference,
            referenced
        );

        Assert.That(loaded != null).Is.True();
        Assert.That(loaded!.IsRight).Is.True();
        Assert.That(ReferenceEquals(loaded.UnwrapRight(), referenced)).Is.True();
    }

    [Test]
    public static void LocalTargetInfoLookModeRoundTripsThingTarget()
    {
        var thing = MakeBareThing(ThingDefOf.Silver);
        LocalTargetInfo target = thing;
        var original = Either<LocalTargetInfo, int>.Left(target);

        var loaded = ScribeRoundTripHarness<LocalTargetInfo, int>.RoundTrip(
            original,
            leftLookMode: LookMode.LocalTargetInfo,
            rightLookMode: LookMode.Value,
            thing
        );

        Assert.That(loaded != null).Is.True();
        Assert.That(loaded!.IsLeft).Is.True();
        Assert.That(ReferenceEquals(loaded.UnwrapLeft().Thing, thing)).Is.True();
    }

    [Test]
    public static void TargetInfoLookModeRoundTripsThingTarget()
    {
        var thing = MakeBareThing(ThingDefOf.Silver);
        TargetInfo target = thing;
        var original = Either<TargetInfo, int>.Left(target);

        var loaded = ScribeRoundTripHarness<TargetInfo, int>.RoundTrip(
            original,
            leftLookMode: LookMode.TargetInfo,
            rightLookMode: LookMode.Value,
            thing
        );

        Assert.That(loaded != null).Is.True();
        Assert.That(loaded!.IsLeft).Is.True();
        Assert.That(ReferenceEquals(loaded.UnwrapLeft().Thing, thing)).Is.True();
    }

    [Test]
    public static void GlobalTargetInfoLookModeRoundTripsThingTarget()
    {
        var thing = MakeBareThing(ThingDefOf.Silver);
        GlobalTargetInfo target = thing;
        var original = Either<GlobalTargetInfo, int>.Left(target);

        var loaded = ScribeRoundTripHarness<GlobalTargetInfo, int>.RoundTrip(
            original,
            leftLookMode: LookMode.GlobalTargetInfo,
            rightLookMode: LookMode.Value,
            thing
        );

        Assert.That(loaded != null).Is.True();
        Assert.That(loaded!.IsLeft).Is.True();
        Assert.That(ReferenceEquals(loaded.UnwrapLeft().Thing, thing)).Is.True();
    }

    [Test]
    public static void BodyPartLookModeRoundTripsNonNullValue()
    {
        var corePart = BodyDefOf.Human.corePart;
        var original = Either<BodyPartRecord, int>.Left(corePart);

        var loaded = ScribeRoundTripHarness<BodyPartRecord, int>.RoundTrip(
            original,
            leftLookMode: LookMode.BodyPart,
            rightLookMode: LookMode.Value
        );

        Assert.That(loaded != null).Is.True();
        Assert.That(loaded!.IsLeft).Is.True();
        Assert.That(ReferenceEquals(loaded.UnwrapLeft(), corePart)).Is.True();
    }

    [Test]
    public static void BodyPartLookModeAcceptsNullValue()
    {
        // Regression guard: the old `value is not BodyPartRecord bodyPartRecord` pattern match
        // rejected null (a type pattern never matches null), so saving/loading a null
        // BodyPartRecord under LookMode.BodyPart threw InvalidOperationException. The fixed
        // pattern (`value is not null and not BodyPartRecord`) lets null through.
        BodyPartRecord? nullPart = null;
        var original = Either<BodyPartRecord?, int>.Left(nullPart);

        var loaded = ScribeRoundTripHarness<BodyPartRecord?, int>.RoundTrip(
            original,
            leftLookMode: LookMode.BodyPart,
            rightLookMode: LookMode.Value
        );

        Assert.That(loaded != null).Is.True();
        Assert.That(loaded!.IsLeft).Is.True();
        Assert.That(loaded.UnwrapLeft() is null).Is.True();
    }
}
