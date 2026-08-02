namespace ilyvion.Laboratory.Collections;

/// <summary>
/// A bounded cache keyed by <typeparamref name="TKey"/> that evicts the least-recently-used
/// entry once <see cref="Capacity"/> is reached.
/// </summary>
/// <remarks>Not thread-safe.</remarks>
[SinceVersion(0, 23, 0)]
public sealed class LruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map;
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LruCache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="capacity">Maximum number of entries the cache holds. Must be positive.</param>
    public LruCache(int capacity)
        : this(capacity, EqualityComparer<TKey>.Default) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LruCache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="capacity">Maximum number of entries the cache holds. Must be positive.</param>
    /// <param name="comparer">The equality comparer used to compare keys.</param>
    public LruCache(int capacity, IEqualityComparer<TKey> comparer)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                capacity,
                "Capacity must be positive."
            );
        }

        Capacity = capacity;
        _map = new Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>>(comparer);
    }

    /// <summary>
    /// Maximum number of entries the cache holds before it starts evicting the
    /// least-recently-used entry.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Current number of entries in the cache.
    /// </summary>
    public int Count => _map.Count;

    /// <summary>
    /// Attempts to get the value associated with <paramref name="key"/>, marking it as the
    /// most-recently-used entry if found.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">The associated value, if found.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _order.Remove(node);
            _order.AddFirst(node);
            value = node.Value.Value;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Determines whether the cache contains <paramref name="key"/>, without affecting its
    /// recency.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    public bool ContainsKey(TKey key) => _map.ContainsKey(key);

    /// <summary>
    /// Adds or updates the value associated with <paramref name="key"/>, marking it as the
    /// most-recently-used entry. If the cache is at <see cref="Capacity"/> and <paramref name="key"/>
    /// is not already present, the least-recently-used entry is evicted first.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The value to associate with <paramref name="key"/>.</param>
    public void Set(TKey key, TValue value)
    {
        if (_map.TryGetValue(key, out var existing))
        {
            _order.Remove(existing);
            _ = _map.Remove(key);
        }
        else if (_map.Count >= Capacity)
        {
            var leastRecentlyUsed = _order.Last;
            if (leastRecentlyUsed != null)
            {
                _order.RemoveLast();
                _ = _map.Remove(leastRecentlyUsed.Value.Key);
            }
        }

        var node = new LinkedListNode<(TKey Key, TValue Value)>((key, value));
        _order.AddFirst(node);
        _map[key] = node;
    }

    /// <summary>
    /// Gets the value associated with <paramref name="key"/>, adding it via <paramref name="valueFactory"/>
    /// first if it isn't already present. Either way, the entry is marked as the most-recently-used one.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="valueFactory">Produces the value to store when <paramref name="key"/> isn't already present.</param>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (valueFactory == null)
        {
            throw new ArgumentNullException(nameof(valueFactory));
        }

        if (TryGetValue(key, out var value))
        {
            return value;
        }

        value = valueFactory(key);
        Set(key, value);
        return value;
    }

    /// <summary>
    /// Removes <paramref name="key"/> from the cache, if present.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> was present and removed; otherwise <see langword="false"/>.</returns>
    public bool Remove(TKey key)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _order.Remove(node);
            _ = _map.Remove(key);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes all entries from the cache.
    /// </summary>
    public void Clear()
    {
        _map.Clear();
        _order.Clear();
    }
}
