// CacheTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class CacheTests
{
    private static int _tick;

    [SetUp]
    public static void SetUp()
    {
        _tick = 0;
        CacheClock.CurrentTick = () => _tick;
    }

    [TearDown]
    public static void TearDown() => CacheClock.ResetCurrentTick();

    // Regression guard for commit b0a00fb: CachedValues<TKey, TValue>.Update() on a key that
    // hadn't been Add()-ed yet used to construct the new CachedValue via the "initial value"
    // constructor without also calling Update() on it, leaving _lastUpdateTick null. That made
    // TryGetValue() immediately consider the freshly-stored value stale (since there's no
    // updater to fall back on), so it returned false right after the value was stored.
    [Test]
    public static void UpdateOnUnregisteredKeyIsImmediatelyRetrievable()
    {
        var cache = new CachedValues<string, int>();

        cache.Update("key", 42);

        Expect.IsTrue(cache.TryGetValue("key", out var value));
        Expect.AreEqual(42, value);
    }

    [Test]
    public static void UpdateOnRegisteredKeyOverwritesValue()
    {
        var cache = new CachedValues<string, int>();
        cache.Add("key", () => -1);

        cache.Update("key", 42);

        Expect.IsTrue(cache.TryGetValue("key", out var value));
        Expect.AreEqual(42, value);
    }

    [Test]
    public static void UpdatedValueStaysFreshWithinUpdateInterval()
    {
        var cache = new CachedValues<string, int>(updateInterval: 10);
        cache.Update("key", 1);

        _tick = 10;

        Expect.IsTrue(cache.TryGetValue("key", out var value));
        Expect.AreEqual(1, value);
    }

    [Test]
    public static void UpdatedValueGoesStaleAfterUpdateInterval()
    {
        var cache = new CachedValues<string, int>(updateInterval: 10);
        cache.Update("key", 1);

        _tick = 11;

        Expect.IsFalse(cache.TryGetValue("key", out _));
    }

    [Test]
    public static void StaleValueWithUpdaterIsTransparentlyRefreshed()
    {
        var callCount = 0;
        var cached = new CachedValue<int>(
            () =>
            {
                callCount++;
                return callCount;
            },
            updateInterval: 10
        );

        Expect.IsTrue(cached.TryGetValue(out var first));
        Expect.AreEqual(1, first);

        _tick = 11;

        Expect.IsTrue(cached.TryGetValue(out var second));
        Expect.AreEqual(2, second);
    }

    [Test]
    public static void InvalidateForcesNextTryGetValueToBeStale()
    {
        var cache = new CachedValues<string, int>(updateInterval: 10);
        cache.Update("key", 1);

        cache.Invalidate("key");

        Expect.IsFalse(cache.TryGetValue("key", out _));
    }

    [Test]
    public static void InitialValueIsImmediatelyRetrievable()
    {
        var cached = new CachedValue<int>(42, updated: true, updateInterval: 10);

        Expect.IsTrue(cached.TryGetValue(out var value));
        Expect.AreEqual(42, value);
    }

    [Test]
    public static void InitialValueGoesStaleAfterUpdateInterval()
    {
        var cached = new CachedValue<int>(42, updated: true, updateInterval: 10);

        _tick = 11;

        Expect.IsFalse(cached.TryGetValue(out _));
    }

    [Test]
    public static void InitialValueWithoutATickManagerIsStaleUntilUpdated()
    {
#pragma warning disable CA2201 // Simulates the null Find.TickManager the real clock dereferences
        CacheClock.CurrentTick = () => throw new NullReferenceException();
#pragma warning restore CA2201
        var cached = new CachedValue<int>(42, updated: true, updateInterval: 10);
        CacheClock.CurrentTick = () => _tick;

        Expect.IsFalse(cached.TryGetValue(out _));
    }

    [Test]
    public static void InitialValueMarkedNotUpdatedIsStaleUntilUpdated()
    {
        var cached = new CachedValue<int>(0, updated: false, updateInterval: 10);

        Expect.IsFalse(cached.TryGetValue(out _));

        _ = cached.Update(5);

        Expect.IsTrue(cached.TryGetValue(out var value));
        Expect.AreEqual(5, value);
    }

    [Test]
    public static void ObsoleteInitialValueConstructorTreatsTheValueAsFresh()
    {
#pragma warning disable CS0618 // Verifies the behavior existing callers get
        var cached = new CachedValue<int>(42, updateInterval: 10);
#pragma warning restore CS0618

        Expect.IsTrue(cached.TryGetValue(out var value));
        Expect.AreEqual(42, value);
    }
}
