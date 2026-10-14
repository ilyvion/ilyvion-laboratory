// Parts of the code:
// Copyright Karel Kroeze, 2018-2020

using System.Diagnostics.CodeAnalysis;
using ilyvion.Laboratory.Coroutines;

namespace ilyvion.Laboratory;

/// <summary>
/// Indirection point for the current game tick, used by <see cref="CachedValue{T}"/> and
/// <see cref="MultiTickCachedValue{T}"/> instead of calling <see cref="TickManager.TicksGame"/>
/// directly so tests can substitute a controllable clock.
/// </summary>
internal static class CacheClock
{
    internal static Func<int> CurrentTick { get; set; } = () => Find.TickManager.TicksGame;

    // Null when there is no tick manager yet (e.g. a value constructed at the main menu).
    internal static int? TryGetCurrentTick()
    {
        try
        {
            return CurrentTick();
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    internal static void ResetCurrentTick() => CurrentTick = () => Find.TickManager.TicksGame;
}

public class CachedValues<TKey, TValue>(int updateInterval = 250)
    where TKey : notnull
{
    private readonly Dictionary<TKey, CachedValue<TValue>> _cacheEntries = [];
    private readonly int updateInterval = updateInterval;

    public TValue? this[TKey index]
    {
        get
        {
            _ = TryGetValue(index, out var value);
            return value;
        }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            Update(index, value);
        }
    }

    public void Add(TKey key, Func<TValue> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);

        var cacheEntry = new CachedValue<TValue>(updater, updateInterval);
        _cacheEntries.Add(key, cacheEntry);
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        if (_cacheEntries.TryGetValue(key, out var cacheEntry))
        {
            return cacheEntry.TryGetValue(out value);
        }

        value = default;
        return false;
    }

    public void Update(TKey key, TValue value)
    {
        if (_cacheEntries.TryGetValue(key, out var cachedValue))
        {
            _ = cachedValue.Update(value);
        }
        else
        {
            var cacheEntry = new CachedValue<TValue>(value, updated: true, updateInterval);
            _ = cacheEntry.Update(value);
            _cacheEntries.Add(key, cacheEntry);
        }
    }

    public void Invalidate(TKey key)
    {
        if (_cacheEntries.TryGetValue(key, out var cachedValue))
        {
            cachedValue.Invalidate();
        }
    }
}

public class CachedValue<T>
{
    private readonly int _updateInterval;
    private readonly Func<T>? _updater;
    private T? _cached;
    private int? _lastUpdateTick;

    [Obsolete(
        "Whether the initial value starts out fresh or stale is easy to get wrong. Use the "
            + "overload that takes an 'updated' argument, or the one that takes an updater."
    )]
    public CachedValue(T initial, int updateInterval = 250)
        : this(initial, updated: true, updateInterval) { }

    /// <summary>
    /// Creates a cached value holding <paramref name="initial"/>.
    /// </summary>
    /// <param name="initial">The value held until the first update.</param>
    /// <param name="updated">
    /// Whether <paramref name="initial"/> is fresh (a real value, valid for
    /// <paramref name="updateInterval"/> ticks) or stale (a placeholder; the first
    /// <see cref="TryGetValue"/> reports a miss so the caller computes and stores the real value).
    /// </param>
    /// <param name="updateInterval">How many ticks a value stays fresh.</param>
    [SinceVersion(0, 25, 0)]
    public CachedValue(T initial, bool updated, int updateInterval = 250)
    {
        _updateInterval = updateInterval;
        _cached = initial;
        _lastUpdateTick = updated ? CacheClock.TryGetCurrentTick() : null;
    }

    public CachedValue(Func<T>? updater, int updateInterval = 250)
    {
        _updateInterval = updateInterval;
        _updater = updater;
        _lastUpdateTick = null;
    }

    public T Value =>
        TryGetValue(out var value) && value != null
            ? value
            : throw new InvalidOperationException(
                "get_Value() on a CachedValue that is out of date, and has no updater."
            );

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        if (
            _lastUpdateTick.HasValue
            && CacheClock.CurrentTick() - _lastUpdateTick.Value <= _updateInterval
        )
        {
            value = _cached ?? throw new InvalidOperationException("_cached was null");
            return true;
        }

        if (_updater != null)
        {
            value = Update() ?? throw new InvalidOperationException("updater produced null value");
            return true;
        }

        _cached = default;
        value = default;
        return false;
    }

    public T Update(T value)
    {
        _cached = value;
        _lastUpdateTick = CacheClock.CurrentTick();
        return _cached;
    }

    public T Update() =>
        _updater != null
            ? Update(_updater())
            : throw new InvalidOperationException(
                $"Calling {nameof(Update)}() on a {nameof(CachedValue<>)} without an updater"
            );

    public void Invalidate() => _lastUpdateTick = null;
}

public class MultiTickCachedValue<T>(
    T initial,
    Func<AnyBoxed<T?>, IEnumerable<IResumeCondition>> updaterCoroutine,
    int updateInterval = 250,
    bool allowNull = false
)
{
    private readonly Func<AnyBoxed<T?>, IEnumerable<IResumeCondition>> _updaterCoroutine =
        updaterCoroutine;
    private readonly int _updateInterval = updateInterval;
    private readonly bool _allowNull = allowNull;
    private int _lastUpdateTick = -1;
    private CoroutineHandle? _updaterCoroutineHandle;

    [Obsolete(
        "This overload will be made private in a future version. Use the parameterless "
            + "DoUpdateIfNeeded() or ForceUpdate() instead."
    )]
    public CoroutineHandle? DoUpdateIfNeeded(bool force = false)
    {
        if (_updaterCoroutineHandle == null)
        {
            if (
                force
                || _lastUpdateTick == -1
                || CacheClock.CurrentTick() - _lastUpdateTick > _updateInterval
            )
            {
                _updaterCoroutineHandle = MultiTickCoroutineManager.StartCoroutine(
                    UpdateValueCoroutine(),
                    () => _lastUpdateTick = CacheClock.CurrentTick(),
                    debugHandle: $"{nameof(MultiTickCachedValue<>)}.{nameof(UpdateValueCoroutine)}"
                );
            }
        }
        return _updaterCoroutineHandle;
    }

    [SinceVersion(0, 23, 0)]
    public CoroutineHandle? DoUpdateIfNeeded() =>
#pragma warning disable CS0618 // Type or member is obsolete
        DoUpdateIfNeeded(force: false);
#pragma warning restore CS0618

    [SinceVersion(0, 23, 0)]
    public CoroutineHandle ForceUpdate() =>
#pragma warning disable CS0618 // Type or member is obsolete
        DoUpdateIfNeeded(force: true)!;
#pragma warning restore CS0618

    private IEnumerable<IResumeCondition> UpdateValueCoroutine()
    {
        using var _ = new DoOnDispose(() => _updaterCoroutineHandle = null);

        AnyBoxed<T?> newCount = new(default);
        yield return _updaterCoroutine(newCount).ResumeWhenOtherCoroutineIsCompleted();

        if (newCount.Value == null && !_allowNull)
        {
            throw new InvalidOperationException(
                $"{nameof(MultiTickCachedValue<>)}'s updater" + "must return a non-null value"
            );
        }

        Value = newCount.Value!;
    }

    public T Value { get; private set; } = initial;
}
