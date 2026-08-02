// EnumerableUtilityTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class EnumerableUtilityTests
{
    [Test]
    public static void LazyOrderBySortsAscending()
    {
        int[] source = [5, 3, 4, 1, 2];

        var result = source.LazyOrderBy(x => x).ToArray();

        Assert.ThatCollection(result).Has.Count(5);
        Assert.That(result[0]).Is.EqualTo(1);
        Assert.That(result[1]).Is.EqualTo(2);
        Assert.That(result[2]).Is.EqualTo(3);
        Assert.That(result[3]).Is.EqualTo(4);
        Assert.That(result[4]).Is.EqualTo(5);
    }

    [Test]
    public static void LazyOrderByDescendingSortsDescending()
    {
        int[] source = [5, 3, 4, 1, 2];

        var result = source.LazyOrderByDescending(x => x).ToArray();

        Assert.That(result[0]).Is.EqualTo(5);
        Assert.That(result[1]).Is.EqualTo(4);
        Assert.That(result[2]).Is.EqualTo(3);
        Assert.That(result[3]).Is.EqualTo(2);
        Assert.That(result[4]).Is.EqualTo(1);
    }

    [Test]
    public static void LazyOrderBySortsLargeSequenceExercisingQuicksortBranch()
    {
        // The iterative quicksort in OrderedEnumerable falls back to insertion sort for
        // ranges of 8 elements or fewer; use a sequence larger than that to exercise the
        // partition/recursive-range branch as well.
        var source = Enumerable.Range(0, 100).Reverse().ToArray();

        var result = source.LazyOrderBy(x => x).ToArray();

        Assert.ThatCollection(result).Has.Count(100);
        for (var i = 0; i < result.Length; i++)
        {
            Assert.That(result[i]).Is.EqualTo(i);
        }
    }

    [Test]
    public static void LazyOrderByIsStableForEqualKeys()
    {
        (int Key, int OriginalIndex)[] source = [(1, 0), (2, 1), (1, 2), (2, 3), (1, 4)];

        var result = source.LazyOrderBy(x => x.Key).ToArray();

        // All "1" keys should retain their relative order, and likewise for "2" keys.
        var onesInOrder = result.Where(x => x.Key == 1).Select(x => x.OriginalIndex).ToArray();
        var twosInOrder = result.Where(x => x.Key == 2).Select(x => x.OriginalIndex).ToArray();

        Assert.ThatCollection(onesInOrder).Has.Count(3);
        Assert.That(onesInOrder[0]).Is.EqualTo(0);
        Assert.That(onesInOrder[1]).Is.EqualTo(2);
        Assert.That(onesInOrder[2]).Is.EqualTo(4);

        Assert.ThatCollection(twosInOrder).Has.Count(2);
        Assert.That(twosInOrder[0]).Is.EqualTo(1);
        Assert.That(twosInOrder[1]).Is.EqualTo(3);
    }

    [Test]
    public static void LazyOrderByThenBySortsByChainedKeys()
    {
        (int Primary, int Secondary)[] source = [(1, 2), (0, 1), (1, 1), (0, 2)];

        var result = source
            .LazyOrderBy(x => x.Primary)
            .CreateOrderedEnumerable(x => x.Secondary, null, false)
            .ToArray();

        Assert.That(result[0]).Is.EqualTo((0, 1));
        Assert.That(result[1]).Is.EqualTo((0, 2));
        Assert.That(result[2]).Is.EqualTo((1, 1));
        Assert.That(result[3]).Is.EqualTo((1, 2));
    }

    [Test]
    public static void LazyOrderByReturnsEmptyForEmptySource()
    {
        var source = Array.Empty<int>();

        var result = source.LazyOrderBy(x => x).ToArray();

        Assert.ThatCollection(result).Is.Empty();
    }

    [Test]
    public static void LazyOrderByReturnsSingleElementForSingletonSource()
    {
        int[] source = [42];

        var result = source.LazyOrderBy(x => x).ToArray();

        Assert.ThatCollection(result).Has.Count(1);
        Assert.That(result[0]).Is.EqualTo(42);
    }
}
