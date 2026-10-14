// ConvertExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class ConvertExtensionsTests
{
    [Test]
    public static void ToHexStringEncodesEveryByteAsTwoUppercaseHexDigits()
    {
        Expect.AreEqual("00010A0FF0FF", Convert.ToHexString([0x00, 0x01, 0x0A, 0x0F, 0xF0, 0xFF]));
        Expect.AreEqual("", Convert.ToHexString([]));
    }

    [Test]
    public static void ToHexStringWithOffsetAndLengthEncodesOnlyThatRange()
    {
        byte[] bytes = [0x12, 0x34, 0x56, 0x78];

        Expect.AreEqual("3456", Convert.ToHexString(bytes, 1, 2));
        Expect.AreEqual("", Convert.ToHexString(bytes, 4, 0));
    }

    [Test]
    public static void ToHexStringThrowsOnARangeOutsideTheArray()
    {
        byte[] bytes = [0x12, 0x34];

        _ = Expect.Throws<ArgumentOutOfRangeException>(() => Convert.ToHexString(bytes, 1, 2));
        _ = Expect.Throws<ArgumentOutOfRangeException>(() => Convert.ToHexString(bytes, -1, 1));
    }
}
