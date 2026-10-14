// LruCacheTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;
using ilyvion.Laboratory.Collections;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class LruCacheTests
{
    [Test]
    public static void SetThenTryGetValueReturnsStoredValue()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);

        var found = cache.TryGetValue("a", out var value);

        Expect.IsTrue(found);
        Expect.AreEqual(1, value);
    }

    [Test]
    public static void TryGetValueOnMissingKeyReturnsFalse()
    {
        var cache = new LruCache<string, int>(2);

        var found = cache.TryGetValue("missing", out _);

        Expect.IsFalse(found);
    }

    [Test]
    public static void SettingBeyondCapacityEvictsLeastRecentlyUsedEntry()
    {
        // The whole point of LruCache over a full-clear cache: only the least-recently-used
        // entry is dropped, everything else survives.
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);

        cache.Set("c", 3);

        Expect.AreEqual(2, cache.Count);
        Expect.IsFalse(cache.ContainsKey("a"));
        Expect.IsTrue(cache.ContainsKey("b"));
        Expect.IsTrue(cache.ContainsKey("c"));
    }

    [Test]
    public static void ReadingAnEntryProtectsItFromEviction()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);
        // Touch "a" so "b" becomes the least-recently-used entry instead.
        _ = cache.TryGetValue("a", out _);

        cache.Set("c", 3);

        Expect.IsTrue(cache.ContainsKey("a"));
        Expect.IsFalse(cache.ContainsKey("b"));
        Expect.IsTrue(cache.ContainsKey("c"));
    }

    [Test]
    public static void SettingAnExistingKeyUpdatesItsValueAndRecency()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);

        cache.Set("a", 100);
        cache.Set("c", 3);

        Expect.AreEqual(2, cache.Count);
        _ = cache.TryGetValue("a", out var value);
        Expect.AreEqual(100, value);
        Expect.IsFalse(cache.ContainsKey("b"));
    }

    [Test]
    public static void GetOrAddCallsFactoryOnlyOnFirstAccess()
    {
        var cache = new LruCache<string, int>(2);
        var calls = 0;

        var first = cache.GetOrAdd(
            "a",
            _ =>
            {
                calls++;
                return 1;
            }
        );
        var second = cache.GetOrAdd(
            "a",
            _ =>
            {
                calls++;
                return 2;
            }
        );

        Expect.AreEqual(1, first);
        Expect.AreEqual(1, second);
        Expect.AreEqual(1, calls);
    }

    [Test]
    public static void RemoveDeletesEntryAndReturnsWhetherItExisted()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);

        var removedExisting = cache.Remove("a");
        var removedMissing = cache.Remove("a");

        Expect.IsTrue(removedExisting);
        Expect.IsFalse(removedMissing);
        Expect.IsFalse(cache.ContainsKey("a"));
    }

    [Test]
    public static void ClearRemovesAllEntries()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);

        cache.Clear();

        Expect.AreEqual(0, cache.Count);
    }

    [Test]
    public static void NonPositiveCapacityThrows() =>
        Expect.Throws<Exception>(() => _ = new LruCache<string, int>(0));
}
