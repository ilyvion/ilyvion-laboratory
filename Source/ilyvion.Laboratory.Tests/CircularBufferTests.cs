// CircularBufferTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;
using ilyvion.Laboratory.Collections;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class CircularBufferTests
{
    [Test]
    public static void NegativeIndexerGetThrows()
    {
        // Regression guard: a negative index used to wrap around via InternalIndex's modulo
        // arithmetic instead of being rejected, silently returning the wrong element.
        var buffer = new CircularBuffer<int>(3);
        buffer.PushBack(1);
        buffer.PushBack(2);
        buffer.PushBack(3);
        // Advance _start past 0 so a negative index would otherwise wrap into bounds.
        buffer.PushBack(4);

        _ = Expect.Throws<Exception>(() => _ = buffer[-1]);
    }

    [Test]
    public static void NegativeIndexerSetThrows()
    {
        var buffer = new CircularBuffer<int>(3);
        buffer.PushBack(1);
        buffer.PushBack(2);
        buffer.PushBack(3);
        buffer.PushBack(4);

        _ = Expect.Throws<Exception>(() => buffer[-1] = 0);
    }
}
