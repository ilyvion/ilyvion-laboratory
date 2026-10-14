// EnumerableUtilityTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class EnumerableUtilityTests
{
    [Test]
    public static void LazyOrderBySortsAscending()
    {
        int[] source = [5, 3, 4, 1, 2];

        var result = source.LazyOrderBy(x => x).ToArray();

        Expect.AreEqual(5, result.Length);
        Expect.AreEqual(1, result[0]);
        Expect.AreEqual(2, result[1]);
        Expect.AreEqual(3, result[2]);
        Expect.AreEqual(4, result[3]);
        Expect.AreEqual(5, result[4]);
    }

    [Test]
    public static void LazyOrderByDescendingSortsDescending()
    {
        int[] source = [5, 3, 4, 1, 2];

        var result = source.LazyOrderByDescending(x => x).ToArray();

        Expect.AreEqual(5, result[0]);
        Expect.AreEqual(4, result[1]);
        Expect.AreEqual(3, result[2]);
        Expect.AreEqual(2, result[3]);
        Expect.AreEqual(1, result[4]);
    }

    [Test]
    public static void LazyOrderBySortsLargeSequenceExercisingQuicksortBranch()
    {
        // The iterative quicksort in OrderedEnumerable falls back to insertion sort for
        // ranges of 8 elements or fewer; use a sequence larger than that to exercise the
        // partition/recursive-range branch as well.
        var source = Enumerable.Range(0, 100).Reverse().ToArray();

        var result = source.LazyOrderBy(x => x).ToArray();

        Expect.AreEqual(100, result.Length);
        for (var i = 0; i < result.Length; i++)
        {
            Expect.AreEqual(i, result[i]);
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

        Expect.AreEqual(3, onesInOrder.Length);
        Expect.AreEqual(0, onesInOrder[0]);
        Expect.AreEqual(2, onesInOrder[1]);
        Expect.AreEqual(4, onesInOrder[2]);

        Expect.AreEqual(2, twosInOrder.Length);
        Expect.AreEqual(1, twosInOrder[0]);
        Expect.AreEqual(3, twosInOrder[1]);
    }

    [Test]
    public static void LazyOrderByThenBySortsByChainedKeys()
    {
        (int Primary, int Secondary)[] source = [(1, 2), (0, 1), (1, 1), (0, 2)];

        var result = source
            .LazyOrderBy(x => x.Primary)
            .CreateOrderedEnumerable(x => x.Secondary, null, false)
            .ToArray();

        Expect.AreEqual((0, 1), result[0]);
        Expect.AreEqual((0, 2), result[1]);
        Expect.AreEqual((1, 1), result[2]);
        Expect.AreEqual((1, 2), result[3]);
    }

    [Test]
    public static void LazyOrderByReturnsEmptyForEmptySource()
    {
        var source = Array.Empty<int>();

        var result = source.LazyOrderBy(x => x).ToArray();

        Expect.IsEmpty(result);
    }

    [Test]
    public static void LazyOrderByReturnsSingleElementForSingletonSource()
    {
        int[] source = [42];

        var result = source.LazyOrderBy(x => x).ToArray();

        Expect.AreEqual(1, result.Length);
        Expect.AreEqual(42, result[0]);
    }
}
