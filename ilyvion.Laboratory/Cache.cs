// Parts of the code:
// Copyright Karel Kroeze, 2018-2020

using System.Diagnostics.CodeAnalysis;
using ilyvion.Laboratory.Coroutines;

namespace ilyvion.Laboratory;

public class CachedValues<TKey, TValue>(int updateInterval = 250)
{
    private readonly Dictionary<TKey, CachedValue<TValue>> _cacheEntries = [];
    private readonly int updateInterval = updateInterval;

    public TValue? this[TKey index]
    {
        get
        {
            TryGetValue(index, out var value);
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
        if (updater == null)
        {
            throw new ArgumentNullException(nameof(updater));
        }

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
            cachedValue.Update(value);
        }
        else
        {
            _cacheEntries.Add(key, new CachedValue<TValue>(value, updateInterval));
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

    public CachedValue(T initial, int updateInterval = 250)
    {
        _updateInterval = updateInterval;
        _cached = initial;
        _lastUpdateTick = null;
    }

    public CachedValue(Func<T>? updater, int updateInterval = 250)
    {
        _updateInterval = updateInterval;
        _updater = updater;
        _lastUpdateTick = null;
    }

    public T Value
    {
        get
        {
            if (TryGetValue(out var value) && value != null)
            {
                return value;
            }

            throw new InvalidOperationException(
                "get_Value() on a CachedValue that is out of date, and has no updater.");
        }
    }

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        if (_lastUpdateTick.HasValue &&
            Find.TickManager.TicksGame - _lastUpdateTick.Value <= _updateInterval)
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
        _lastUpdateTick = Find.TickManager.TicksGame;
        return _cached;
    }

    public T Update()
    {
        return _updater != null
            ? Update(_updater())
            : throw new InvalidOperationException(
                $"Calling {nameof(Update)}() on a {nameof(CachedValue<T>)} without an updater");
    }

    public void Invalidate()
    {
        _lastUpdateTick = null;
    }
}

public class MultiTickCachedValue<T>
{
    private T _cached;
    private readonly Func<AnyBoxed<T?>, IEnumerable<IResumeCondition>> _updaterCoroutine;
    private readonly int _updateInterval;
    private readonly bool _allowNull;
    private int _lastUpdateTick = -1;
    private CoroutineHandle? _updaterCoroutineHandle;

    public MultiTickCachedValue(
        T initial, Func<AnyBoxed<T?>,
        IEnumerable<IResumeCondition>> updaterCoroutine,
        int updateInterval = 250,
        bool allowNull = false)
    {
        _cached = initial;
        _updaterCoroutine = updaterCoroutine;
        _updateInterval = updateInterval;
        _allowNull = allowNull;
    }

    // NOTE: DoesNotReturnIf is *technically* incorrect here; but there is no DoesNotReturnNullIf,
    // which is what I'd really want, and the behavior of the null analysis for the two would be
    // identical anyway, which is why I use it.
    public CoroutineHandle? DoUpdateIfNeeded([DoesNotReturnIf(true)] bool force = false)
    {
        if (_updaterCoroutineHandle == null)
        {
            if (force || _lastUpdateTick == -1
                || Find.TickManager.TicksGame - _lastUpdateTick > _updateInterval)
            {
                _updaterCoroutineHandle =
                    MultiTickCoroutineManager.StartCoroutine(
                        UpdateValueCoroutine(),
                        () => _lastUpdateTick = Find.TickManager.TicksGame,
                        debugHandle:
                            $"{nameof(MultiTickCachedValue<T>)}.{nameof(UpdateValueCoroutine)}");
            }
        }
        return _updaterCoroutineHandle;
    }

    private IEnumerable<IResumeCondition> UpdateValueCoroutine()
    {
        using var _ = new DoOnDispose(() => _updaterCoroutineHandle = null);

        AnyBoxed<T?> newCount = new(default);
        yield return _updaterCoroutine(newCount)
            .ResumeWhenOtherCoroutineIsCompleted();

        if (newCount.Value == null && !_allowNull)
        {
            throw new InvalidOperationException($"{nameof(MultiTickCachedValue<T>)}'s updater" +
                "must return a non-null value");
        }

        _cached = newCount.Value!;
    }

    public T Value => _cached;
}
