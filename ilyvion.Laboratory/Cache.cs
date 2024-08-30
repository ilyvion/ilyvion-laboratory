// Parts of the code:
// Copyright Karel Kroeze, 2018-2020

using System.Diagnostics.CodeAnalysis;

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
    private int? _timeSet;

    public CachedValue(T initial, int updateInterval = 250)
    {
        _updateInterval = updateInterval;
        _cached = initial;
        _timeSet = null;
    }

    public CachedValue(Func<T>? updater, int updateInterval = 250)
    {
        _updateInterval = updateInterval;
        _updater = updater;
        _timeSet = null;
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
        if (_timeSet.HasValue && Find.TickManager.TicksGame - _timeSet.Value <= _updateInterval)
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
        _timeSet = Find.TickManager.TicksGame;
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
        _timeSet = null;
    }
}
