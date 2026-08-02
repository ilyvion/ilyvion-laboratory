// ScribeCircularBufferTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using ilyvion.Laboratory.Collections;
using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

// Regression suite for the Scribe_CircularBuffer fixes: previously the outer lookMode passed to
// Look() was never forwarded to the inner element scribe (elements were always scribed with
// LookMode.Undefined regardless of what the caller asked for), and LookMode.Reference buffers
// were unconditionally nulled out during the ResolvingCrossRefs pass instead of being populated
// with the resolved references. A corrupt/hand-edited save with capacity 0 or a missing "values"
// node also used to throw/NRE instead of loading a recoverable empty buffer.
[TestSuite]
internal static class ScribeCircularBufferTests
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

    /// <summary>
    /// Drives <see cref="Scribe_CircularBuffer.Look{T}(ref CircularBuffer{T}?, string, LookMode)"/>
    /// through a full save, then a load/resolve-cross-refs cycle, entirely in memory.
    /// </summary>
    private sealed class CircularBufferRoundTripHarness<T>(
        string label,
        LookMode lookMode,
        IReadOnlyList<ILoadReferenceable> extraCrossRefs
    ) : IExposable
    {
        public CircularBuffer<T>? Value;

        public void ExposeData()
        {
            Scribe_CircularBuffer.Look(ref Value, label, lookMode);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (var extraCrossRef in extraCrossRefs)
                {
                    Scribe.saver.loadIDsErrorsChecker.RegisterDeepSaved(extraCrossRef, label);
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (lookMode == LookMode.Reference)
                {
                    Scribe.loader.crossRefs.RegisterForCrossRefResolve(this);
                }
                foreach (var extraCrossRef in extraCrossRefs)
                {
                    Scribe.loader.crossRefs.RegisterForCrossRefResolve((IExposable)extraCrossRef);
                }
            }
        }

        public static CircularBuffer<T>? RoundTrip(
            CircularBuffer<T>? value,
            LookMode lookMode = LookMode.Undefined,
            params ILoadReferenceable[] extraCrossRefs
        )
        {
            const string label = "buffer";

            using var memory = new MemoryStream();
            using (var nonClosing = new NonClosingStream(memory))
            {
                CustomStreamScribeSaver.InitSaving(nonClosing, "root");
                try
                {
                    var saveHarness = new CircularBufferRoundTripHarness<T>(
                        label,
                        lookMode,
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
            var loadHarness = new CircularBufferRoundTripHarness<T>(
                label,
                lookMode,
                extraCrossRefs
            );
            try
            {
                using (var reader = new StreamReader(memory))
                {
                    CustomStreamReaderScribeLoader.InitLoading(reader);
                }
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
    public static void ValueLookModeRoundTripsElementsAndCapacity()
    {
        var original = new CircularBuffer<int>(5);
        original.PushBack(1);
        original.PushBack(2);
        original.PushBack(3);

        var loaded = CircularBufferRoundTripHarness<int>.RoundTrip(
            original,
            lookMode: LookMode.Value
        );

        Assert.That(loaded != null).Is.True();
        Assert.That(loaded!.Capacity).Is.EqualTo(5);
        Assert.ThatCollection(loaded).Has.Count(3);
        Assert.ThatCollection(loaded).Does.Contain(1);
        Assert.ThatCollection(loaded).Does.Contain(2);
        Assert.ThatCollection(loaded).Does.Contain(3);
    }

    [Test]
    public static void ReferenceLookModeRoundTripsElements()
    {
        // Regression guard: previously LookMode.Reference buffers were unconditionally nulled
        // out during ResolvingCrossRefs instead of being populated with the resolved values, so
        // the loaded buffer was always null.
        var first = new FakeReferenceable();
        var second = new FakeReferenceable();
        var original = new CircularBuffer<FakeReferenceable>(4);
        original.PushBack(first);
        original.PushBack(second);

        var loaded = CircularBufferRoundTripHarness<FakeReferenceable>.RoundTrip(
            original,
            lookMode: LookMode.Reference,
            first,
            second
        );

        Assert.That(loaded != null).Is.True();
        Assert.ThatCollection(loaded).Has.Count(2);
        Assert.That(ReferenceEquals(loaded![0], first)).Is.True();
        Assert.That(ReferenceEquals(loaded[1], second)).Is.True();
    }

    [Test]
    public static void NullBufferRoundTripsAsNull()
    {
        var loaded = CircularBufferRoundTripHarness<int>.RoundTrip(null, lookMode: LookMode.Value);

        Assert.That(loaded == null).Is.True();
    }

    [Test]
    public static void LoadingCorruptZeroCapacityWithMissingValuesNodeRecoversInsteadOfThrowing()
    {
        // Regression guard: a hand-edited/corrupt save with capacity 0 used to throw inside the
        // CircularBuffer constructor, and a save missing the "values" node entirely used to NRE
        // on serialized.values.Count. Both should now recover with an empty, capacity-1 buffer.
        const string label = "buffer";

        using var memory = new MemoryStream();
        using (
            var writer = new StreamWriter(memory, System.Text.Encoding.UTF8, 1024, leaveOpen: true)
        )
        {
            writer.Write($"<root><{label}><capacity>0</capacity></{label}></root>");
            writer.Flush();
        }
        memory.Position = 0;

        var harness = new CircularBufferRoundTripHarness<int>(label, LookMode.Value, []);
        try
        {
            using (var reader = new StreamReader(memory))
            {
                CustomStreamReaderScribeLoader.InitLoading(reader);
            }
            Scribe.loader.curParent = harness;
            harness.ExposeData();
            Scribe.loader.FinalizeLoading();
        }
        finally
        {
            if (Scribe.mode != LoadSaveMode.Inactive)
            {
                Scribe.ForceStop();
            }
        }

        Assert.That(harness.Value != null).Is.True();
        Assert.That(harness.Value!.Capacity).Is.EqualTo(1);
        Assert.That(harness.Value.Size).Is.EqualTo(0);
    }
}
