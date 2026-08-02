#if !v1_6
namespace ilyvion.Laboratory;

internal sealed class ConditionalWeakTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : class
    where TValue : class
{
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<
        TKey,
        TValue
    > innerConditionalWeakTable = new();

    // Keyed by the key's identity hash code (not TKey itself, which would keep it alive
    // strongly) so tracking/deduping a key is an O(1) bucket lookup instead of an O(n) scan.
    private readonly Dictionary<int, List<System.WeakReference<TKey>>> keyReferencesByHash = [];

    public void Add(TKey key, TValue value)
    {
        innerConditionalWeakTable.Add(key, value);
        TrackKey(key);
    }

    public void AddOrUpdate(TKey key, TValue value)
    {
        if (IsTracked(key))
        {
            _ = Remove(key);
        }
        Add(key, value);
    }

    private bool IsTracked(TKey key)
    {
        var hash = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(key);
        if (!keyReferencesByHash.TryGetValue(hash, out var bucket))
        {
            return false;
        }
        foreach (var wr in bucket)
        {
            if (wr.TryGetTarget(out var existingKey) && existingKey == key)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Prunes dead weak references in <paramref name="key"/>'s bucket and records
    /// <paramref name="key"/> if it isn't already tracked by a live reference.
    /// </summary>
    private void TrackKey(TKey key)
    {
        var hash = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(key);
        if (!keyReferencesByHash.TryGetValue(hash, out var bucket))
        {
            bucket = [];
            keyReferencesByHash[hash] = bucket;
        }

        _ = bucket.RemoveAll(wr => !wr.TryGetTarget(out _));
        foreach (var wr in bucket)
        {
            if (wr.TryGetTarget(out var existingKey) && existingKey == key)
            {
                return;
            }
        }
        bucket.Add(new System.WeakReference<TKey>(key));
    }

    public void Clear()
    {
        foreach (var bucket in keyReferencesByHash.Values)
        {
            foreach (var wr in bucket)
            {
                if (wr.TryGetTarget(out var key))
                {
                    _ = innerConditionalWeakTable.Remove(key);
                }
            }
        }
        keyReferencesByHash.Clear();
    }

    public TValue GetOrCreateValue(TKey key)
    {
        TrackKey(key);
        return innerConditionalWeakTable.GetOrCreateValue(key);
    }

    public TValue GetValue(
        TKey key,
        System.Runtime.CompilerServices.ConditionalWeakTable<
            TKey,
            TValue
        >.CreateValueCallback createValueCallback
    )
    {
        TrackKey(key);
        return innerConditionalWeakTable.GetValue(key, createValueCallback);
    }

    public bool Remove(TKey key)
    {
        var hash = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(key);
        if (keyReferencesByHash.TryGetValue(hash, out var bucket))
        {
            _ = bucket.RemoveAll(wr => wr.TryGetTarget(out var k) && k == key);
            if (bucket.Count == 0)
            {
                _ = keyReferencesByHash.Remove(hash);
            }
        }
        return innerConditionalWeakTable.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value) =>
        innerConditionalWeakTable.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var bucket in keyReferencesByHash.Values)
        {
            foreach (var wr in bucket)
            {
                if (wr.TryGetTarget(out var key))
                {
                    if (innerConditionalWeakTable.TryGetValue(key, out var value))
                    {
                        yield return new KeyValuePair<TKey, TValue>(key, value);
                    }
                }
            }
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
#endif
