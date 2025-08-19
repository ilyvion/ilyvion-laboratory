#if !v1_6
namespace ilyvion.Laboratory;

internal sealed class ConditionalWeakTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : class
    where TValue : class
{
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue> innerConditionalWeakTable = new();
    private readonly HashSet<System.WeakReference<TKey>> keyReferences = [];

    public void Add(TKey key, TValue value)
    {
        innerConditionalWeakTable.Add(key, value);
        _ = keyReferences.Add(new System.WeakReference<TKey>(key));
    }

    public void AddOrUpdate(TKey key, TValue value)
    {
        if (keyReferences.Any(wr => wr.TryGetTarget(out var existingKey) && existingKey == key))
        {
            _ = Remove(key);
            Add(key, value);
        }
        else
        {
            Add(key, value);
        }
    }

    public void Clear()
    {
        foreach (var wr in keyReferences)
        {
            if (wr.TryGetTarget(out var key))
            {
                _ = innerConditionalWeakTable.Remove(key);
            }
        }
        keyReferences.Clear();
    }

    public TValue GetOrCreateValue(TKey key)
    {
        _ = keyReferences.Add(new System.WeakReference<TKey>(key));
        return innerConditionalWeakTable.GetOrCreateValue(key);
    }

    public TValue GetValue(TKey key, System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.CreateValueCallback createValueCallback)
    {
        _ = keyReferences.Add(new System.WeakReference<TKey>(key));
        return innerConditionalWeakTable.GetValue(key, createValueCallback);
    }

    public bool Remove(TKey key)
    {
        _ = keyReferences.RemoveWhere(wr => wr.TryGetTarget(out var k) && k == key);
        return innerConditionalWeakTable.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return innerConditionalWeakTable.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var wr in keyReferences)
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

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
#endif
