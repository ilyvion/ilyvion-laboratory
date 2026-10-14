// UtilsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class UtilsTests
{
    // Regression guard for commit 6964fbc: CeilToPrecision() clamped the exponent to a minimum
    // of 1, so any precision request finer than "round to the nearest 10" was ignored and small
    // values always rounded to the nearest 10 instead of the requested precision.
    [Test]
    public static void CeilToPrecisionHonorsPrecisionForSmallValues()
    {
        var result = Utils.CeilToPrecision(4, precision: 0);

        Expect.AreEqual(5, result);
    }

    [Test]
    public static void CeilToPrecisionRoundsToNearestUnitByDefault()
    {
        var result = Utils.CeilToPrecision(25);

        Expect.AreEqual(26, result);
    }

    [Test]
    public static void CeilToPrecisionRoundsUpToUnitForLargeValue()
    {
        var result = Utils.CeilToPrecision(1234, precision: 2);

        Expect.AreEqual(1240, result);
    }

    // Regression guard for commit 6964fbc: FormatCount() looped while
    // i < unitSuffixes.Length, which let i reach unitSuffixes.Length and crash with an
    // IndexOutOfRangeException once a value was large enough to run past the largest suffix (G).
    [Test]
    public static void FormatCountDoesNotCrashPastLargestSuffix()
    {
        var result = Utils.FormatCount(1_000_000_000_000f, "s");

        Expect.AreEqual("1000 Gs", result);
    }

    [Test]
    public static void FormatCountUsesLargestSuffixForValueBelowThreshold()
    {
        var result = Utils.FormatCount(500f, "s");

        Expect.AreEqual("500 s", result);
    }

    [Test]
    public static void FormatCountConvertsToNextSuffixAboveThreshold()
    {
        var result = Utils.FormatCount(1500f, "s");

        Expect.AreEqual("1.5 ks", result);
    }
}
