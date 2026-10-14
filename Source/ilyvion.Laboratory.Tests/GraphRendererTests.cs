// GraphRendererTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;
using ilyvion.Laboratory.UI;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class GraphRendererTests
{
    [Test]
    public static void ValidateTargetDataLengthAllowsNullTargetData()
    {
        int[][] data =
        [
            [1, 2],
            [3, 4],
        ];

        GraphRenderer.ValidateTargetDataLength(data, null);
    }

    [Test]
    public static void ValidateTargetDataLengthAllowsMatchingLengths()
    {
        int[][] data =
        [
            [1, 2],
            [3, 4],
        ];
        int[]?[] targetData =
        [
            [1, 2],
            null,
        ];

        GraphRenderer.ValidateTargetDataLength(data, targetData);
    }

    // Regression guard for commit 20d259d: the length check used to be gated behind
    // DrawTargetLine, so a mismatched targetData length went unvalidated whenever
    // DrawTargetLine was false, only to blow up later when the data was consumed.
    [Test]
    public static void ValidateTargetDataLengthThrowsOnMismatchRegardlessOfDrawTargetLine()
    {
        int[][] data =
        [
            [1, 2],
            [3, 4],
        ];
        int[]?[] targetData =
        [
            [1, 2],
        ];

        _ = Expect.Throws<Exception>(() =>
            GraphRenderer.ValidateTargetDataLength(data, targetData)
        );
    }

    // Regression guard for commit 20d259d: widthUnit used to divide by (entries - 1) with no
    // floor, so a single-entry graph divided by zero.
    [Test]
    public static void ComputeWidthUnitDoesNotDivideByZeroForSingleEntry()
    {
        var result = GraphRenderer.ComputeWidthUnit(100f, 1);

        Expect.AreEqual(100f, result);
    }

    [Test]
    public static void ComputeWidthUnitDividesEvenlyAcrossEntries()
    {
        var result = GraphRenderer.ComputeWidthUnit(100f, 5);

        Expect.AreEqual(25f, result);
    }

    // Regression guard for commit 20d259d: the mouse-over closest-series search used to filter
    // shownData down to only the series that had an entry at unitXPosition before indexing back
    // into it by position, which shifted indices whenever an earlier series was shorter than the
    // one actually closest to the cursor, misattributing the tooltip to the wrong series.
    [Test]
    public static void FindClosestSeriesAttributesCorrectSeriesWhenEarlierSeriesHasFewerEntries()
    {
        int[][] shownData =
        [
            [10, 10],
            [10, 10, 10, 10],
        ];
        int[]?[] shownTargetData = [null, null];

        var (seriesIndex, isTarget) = GraphRenderer.FindClosestSeries(
            shownData,
            shownTargetData,
            unitXPosition: 3,
            unitYPosition: 10,
            drawTargetLine: false
        );

        Expect.AreEqual(1, seriesIndex);
        Expect.IsFalse(isTarget);
    }

    [Test]
    public static void FindClosestSeriesPrefersValueOverTargetOnTie()
    {
        int[][] shownData =
        [
            [5],
        ];
        int[]?[] shownTargetData =
        [
            [5],
        ];

        var (seriesIndex, isTarget) = GraphRenderer.FindClosestSeries(
            shownData,
            shownTargetData,
            unitXPosition: 0,
            unitYPosition: 5,
            drawTargetLine: true
        );

        Expect.AreEqual(0, seriesIndex);
        Expect.IsFalse(isTarget);
    }

    [Test]
    public static void FindClosestSeriesPicksTargetWhenStrictlyCloserThanValue()
    {
        int[][] shownData =
        [
            [0],
        ];
        int[]?[] shownTargetData =
        [
            [10],
        ];

        var (seriesIndex, isTarget) = GraphRenderer.FindClosestSeries(
            shownData,
            shownTargetData,
            unitXPosition: 0,
            unitYPosition: 9,
            drawTargetLine: true
        );

        Expect.AreEqual(0, seriesIndex);
        Expect.IsTrue(isTarget);
    }

    [Test]
    public static void FindClosestSeriesIgnoresTargetWhenDrawTargetLineIsFalse()
    {
        int[][] shownData =
        [
            [0],
        ];
        int[]?[] shownTargetData =
        [
            [10],
        ];

        var (seriesIndex, isTarget) = GraphRenderer.FindClosestSeries(
            shownData,
            shownTargetData,
            unitXPosition: 0,
            unitYPosition: 9,
            drawTargetLine: false
        );

        Expect.AreEqual(0, seriesIndex);
        Expect.IsFalse(isTarget);
    }
}
