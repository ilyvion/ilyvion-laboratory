// NumericFieldTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;
using ilyvion.Laboratory.UI;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class NumericFieldTests
{
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

    private sealed class SettingsHarness : IExposable
    {
        public NumericField<int> Port = new(1000);

        public void ExposeData() => Scribe_Values.Look(ref Port.Value, "port", 1000);
    }

    [Test]
    public static void BufferShowsInitialValue()
    {
        var field = new NumericField<int>(5);

        Expect.AreEqual(5, field.Value);
        Expect.AreEqual("5", field.Buffer);
    }

    [Test]
    public static void BufferUsesFormat()
    {
        var field = new NumericField<float>(3f, "F1");

        Expect.AreEqual(3f.ToString("F1", CultureInfo.CurrentCulture), field.Buffer);
    }

    [Test]
    public static void BufferIsRebuiltWhenValueIsAssigned()
    {
        var field = new NumericField<int>(1);
        _ = field.Buffer;

        field.Value = 42;

        Expect.AreEqual("42", field.Buffer);
    }

    [Test]
    public static void BufferIsRebuiltWhenSettingsAreLoaded()
    {
        using var memory = new MemoryStream();
        using (var nonClosing = new NonClosingStream(memory))
        {
            CustomStreamScribeSaver.InitSaving(nonClosing, "root");
            try
            {
                var saved = new SettingsHarness();
                saved.Port.Value = 3294;
                saved.ExposeData();
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
        var loaded = new SettingsHarness();
        Expect.AreEqual("1000", loaded.Port.Buffer);
        try
        {
            using (var reader = new StreamReader(memory))
            {
                CustomStreamReaderScribeLoader.InitLoading(reader);
            }
            Scribe.loader.curParent = loaded;
            loaded.ExposeData();
            Scribe.loader.FinalizeLoading();
        }
        finally
        {
            if (Scribe.mode != LoadSaveMode.Inactive)
            {
                Scribe.ForceStop();
            }
        }

        Expect.AreEqual(3294, loaded.Port.Value);
        Expect.AreEqual("3294", loaded.Port.Buffer);
    }
}
