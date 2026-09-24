// CacheTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class CacheTests
{
    private static int _tick;

    [BeforeEach]
    public static void SetUp()
    {
        _tick = 0;
        CacheClock.CurrentTick = () => _tick;
    }

    [AfterEach]
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

        Assert.That(cache.TryGetValue("key", out var value)).Is.True();
        Assert.That(value).Is.EqualTo(42);
    }

    [Test]
    public static void UpdateOnRegisteredKeyOverwritesValue()
    {
        var cache = new CachedValues<string, int>();
        cache.Add("key", () => -1);

        cache.Update("key", 42);

        Assert.That(cache.TryGetValue("key", out var value)).Is.True();
        Assert.That(value).Is.EqualTo(42);
    }

    [Test]
    public static void UpdatedValueStaysFreshWithinUpdateInterval()
    {
        var cache = new CachedValues<string, int>(updateInterval: 10);
        cache.Update("key", 1);

        _tick = 10;

        Assert.That(cache.TryGetValue("key", out var value)).Is.True();
        Assert.That(value).Is.EqualTo(1);
    }

    [Test]
    public static void UpdatedValueGoesStaleAfterUpdateInterval()
    {
        var cache = new CachedValues<string, int>(updateInterval: 10);
        cache.Update("key", 1);

        _tick = 11;

        Assert.That(cache.TryGetValue("key", out _)).Is.False();
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

        Assert.That(cached.TryGetValue(out var first)).Is.True();
        Assert.That(first).Is.EqualTo(1);

        _tick = 11;

        Assert.That(cached.TryGetValue(out var second)).Is.True();
        Assert.That(second).Is.EqualTo(2);
    }

    [Test]
    public static void InvalidateForcesNextTryGetValueToBeStale()
    {
        var cache = new CachedValues<string, int>(updateInterval: 10);
        cache.Update("key", 1);

        cache.Invalidate("key");

        Assert.That(cache.TryGetValue("key", out _)).Is.False();
    }

    [Test]
    public static void InitialValueIsImmediatelyRetrievable()
    {
        var cached = new CachedValue<int>(42, updated: true, updateInterval: 10);

        Assert.That(cached.TryGetValue(out var value)).Is.True();
        Assert.That(value).Is.EqualTo(42);
    }

    [Test]
    public static void InitialValueGoesStaleAfterUpdateInterval()
    {
        var cached = new CachedValue<int>(42, updated: true, updateInterval: 10);

        _tick = 11;

        Assert.That(cached.TryGetValue(out _)).Is.False();
    }

    [Test]
    public static void InitialValueWithoutATickManagerIsStaleUntilUpdated()
    {
#pragma warning disable CA2201 // Simulates the null Find.TickManager the real clock dereferences
        CacheClock.CurrentTick = () => throw new NullReferenceException();
#pragma warning restore CA2201
        var cached = new CachedValue<int>(42, updated: true, updateInterval: 10);
        CacheClock.CurrentTick = () => _tick;

        Assert.That(cached.TryGetValue(out _)).Is.False();
    }

    [Test]
    public static void InitialValueMarkedNotUpdatedIsStaleUntilUpdated()
    {
        var cached = new CachedValue<int>(0, updated: false, updateInterval: 10);

        Assert.That(cached.TryGetValue(out _)).Is.False();

        _ = cached.Update(5);

        Assert.That(cached.TryGetValue(out var value)).Is.True();
        Assert.That(value).Is.EqualTo(5);
    }

    [Test]
    public static void ObsoleteInitialValueConstructorTreatsTheValueAsFresh()
    {
#pragma warning disable CS0618 // Verifies the behavior existing callers get
        var cached = new CachedValue<int>(42, updateInterval: 10);
#pragma warning restore CS0618

        Assert.That(cached.TryGetValue(out var value)).Is.True();
        Assert.That(value).Is.EqualTo(42);
    }
}
