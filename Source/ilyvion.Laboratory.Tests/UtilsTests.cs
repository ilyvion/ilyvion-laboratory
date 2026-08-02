// UtilsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class UtilsTests
{
    // Regression guard for commit 6964fbc: CeilToPrecision() clamped the exponent to a minimum
    // of 1, so any precision request finer than "round to the nearest 10" was ignored and small
    // values always rounded to the nearest 10 instead of the requested precision.
    [Test]
    public static void CeilToPrecisionHonorsPrecisionForSmallValues()
    {
        var result = Utils.CeilToPrecision(4, precision: 0);

        Assert.That(result).Is.EqualTo(5);
    }

    [Test]
    public static void CeilToPrecisionRoundsToNearestUnitByDefault()
    {
        var result = Utils.CeilToPrecision(25);

        Assert.That(result).Is.EqualTo(26);
    }

    [Test]
    public static void CeilToPrecisionRoundsUpToUnitForLargeValue()
    {
        var result = Utils.CeilToPrecision(1234, precision: 2);

        Assert.That(result).Is.EqualTo(1240);
    }

    // Regression guard for commit 6964fbc: FormatCount() looped while
    // i < unitSuffixes.Length, which let i reach unitSuffixes.Length and crash with an
    // IndexOutOfRangeException once a value was large enough to run past the largest suffix (G).
    [Test]
    public static void FormatCountDoesNotCrashPastLargestSuffix()
    {
        var result = Utils.FormatCount(1_000_000_000_000f, "s");

        Assert.That(result).Is.EqualTo("1000 Gs");
    }

    [Test]
    public static void FormatCountUsesLargestSuffixForValueBelowThreshold()
    {
        var result = Utils.FormatCount(500f, "s");

        Assert.That(result).Is.EqualTo("500 s");
    }

    [Test]
    public static void FormatCountConvertsToNextSuffixAboveThreshold()
    {
        var result = Utils.FormatCount(1500f, "s");

        Assert.That(result).Is.EqualTo("1.5 ks");
    }
}
