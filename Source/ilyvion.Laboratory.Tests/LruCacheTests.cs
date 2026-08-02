// LruCacheTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using ilyvion.Laboratory.Collections;
using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class LruCacheTests
{
    [Test]
    public static void SetThenTryGetValueReturnsStoredValue()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);

        var found = cache.TryGetValue("a", out var value);

        Assert.That(found).Is.True();
        Assert.That(value).Is.EqualTo(1);
    }

    [Test]
    public static void TryGetValueOnMissingKeyReturnsFalse()
    {
        var cache = new LruCache<string, int>(2);

        var found = cache.TryGetValue("missing", out _);

        Assert.That(found).Is.False();
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

        Assert.That(cache.Count).Is.EqualTo(2);
        Assert.That(cache.ContainsKey("a")).Is.False();
        Assert.That(cache.ContainsKey("b")).Is.True();
        Assert.That(cache.ContainsKey("c")).Is.True();
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

        Assert.That(cache.ContainsKey("a")).Is.True();
        Assert.That(cache.ContainsKey("b")).Is.False();
        Assert.That(cache.ContainsKey("c")).Is.True();
    }

    [Test]
    public static void SettingAnExistingKeyUpdatesItsValueAndRecency()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);

        cache.Set("a", 100);
        cache.Set("c", 3);

        Assert.That(cache.Count).Is.EqualTo(2);
        _ = cache.TryGetValue("a", out var value);
        Assert.That(value).Is.EqualTo(100);
        Assert.That(cache.ContainsKey("b")).Is.False();
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

        Assert.That(first).Is.EqualTo(1);
        Assert.That(second).Is.EqualTo(1);
        Assert.That(calls).Is.EqualTo(1);
    }

    [Test]
    public static void RemoveDeletesEntryAndReturnsWhetherItExisted()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);

        var removedExisting = cache.Remove("a");
        var removedMissing = cache.Remove("a");

        Assert.That(removedExisting).Is.True();
        Assert.That(removedMissing).Is.False();
        Assert.That(cache.ContainsKey("a")).Is.False();
    }

    [Test]
    public static void ClearRemovesAllEntries()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);

        cache.Clear();

        Assert.That(cache.Count).Is.EqualTo(0);
    }

    [Test]
    public static void NonPositiveCapacityThrows() =>
        Assert.ThatFunc(() => new LruCache<string, int>(0)).Does.Throw();
}
