// WeakTable.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.Diagnostics.CodeAnalysis;

namespace ilyvion.Laboratory;

/// <summary>
/// A <see cref="System.Runtime.CompilerServices.ConditionalWeakTable{TKey, TValue}"/> with
/// <see cref="AddOrUpdate"/>, <see cref="Clear"/> and enumeration on every RimWorld version,
/// including the ones whose runtime lacks them. Keys are held weakly; an entry goes away when
/// its key is garbage collected.
/// </summary>
/// <typeparam name="TKey">The key type, compared by reference.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
[SinceVersion(0, 27, 0)]
public sealed class WeakTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : class
    where TValue : class
{
#if v1_6_OR_GREATER
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<
        TKey,
        TValue
    > innerConditionalWeakTable = [];
#else
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<
        TKey,
        TValue
    > innerConditionalWeakTable = new();

    // Keyed by the key's identity hash code (not TKey itself, which would keep it alive
    // strongly) so tracking/deduping a key is an O(1) bucket lookup instead of an O(n) scan.
    private readonly Dictionary<int, List<System.WeakReference<TKey>>> keyReferencesByHash = [];
#endif

    /// <summary>Adds an entry.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentException">The key is already in the table.</exception>
    public void Add(TKey key, TValue value)
    {
        innerConditionalWeakTable.Add(key, value);
#if !v1_6_OR_GREATER
        TrackKey(key);
#endif
    }

    /// <summary>Adds an entry, or replaces the value if the key is already in the table.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void AddOrUpdate(TKey key, TValue value)
    {
#if v1_6_OR_GREATER
        innerConditionalWeakTable.AddOrUpdate(key, value);
#else
        if (IsTracked(key))
        {
            _ = Remove(key);
        }
        Add(key, value);
#endif
    }

#if !v1_6_OR_GREATER
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
#endif

    /// <summary>Removes every entry.</summary>
    public void Clear()
    {
#if v1_6_OR_GREATER
        innerConditionalWeakTable.Clear();
#else
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
#endif
    }

    /// <summary>
    /// Gets the value for <paramref name="key"/>, adding one made by
    /// <typeparamref name="TValue"/>'s parameterless constructor if the key isn't in the table.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value for <paramref name="key"/>.</returns>
    public TValue GetOrCreateValue(TKey key)
    {
#if !v1_6_OR_GREATER
        TrackKey(key);
#endif
        return innerConditionalWeakTable.GetOrCreateValue(key);
    }

    /// <summary>
    /// Gets the value for <paramref name="key"/>, adding one made by
    /// <paramref name="createValueCallback"/> if the key isn't in the table.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="createValueCallback">Makes the value for a key that isn't in the table.</param>
    /// <returns>The value for <paramref name="key"/>.</returns>
    public TValue GetValue(
        TKey key,
        System.Runtime.CompilerServices.ConditionalWeakTable<
            TKey,
            TValue
        >.CreateValueCallback createValueCallback
    )
    {
#if !v1_6_OR_GREATER
        TrackKey(key);
#endif
        return innerConditionalWeakTable.GetValue(key, createValueCallback);
    }

    /// <summary>Removes the entry for <paramref name="key"/>.</summary>
    /// <param name="key">The key.</param>
    /// <returns>Whether the key was in the table.</returns>
    public bool Remove(TKey key)
    {
#if !v1_6_OR_GREATER
        var hash = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(key);
        if (keyReferencesByHash.TryGetValue(hash, out var bucket))
        {
            _ = bucket.RemoveAll(wr => wr.TryGetTarget(out var k) && k == key);
            if (bucket.Count == 0)
            {
                _ = keyReferencesByHash.Remove(hash);
            }
        }
#endif
        return innerConditionalWeakTable.Remove(key);
    }

    /// <summary>Gets the value for <paramref name="key"/>.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value, if the key is in the table.</param>
    /// <returns>Whether the key is in the table.</returns>
    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value) =>
        innerConditionalWeakTable.TryGetValue(key, out value);

    /// <summary>Enumerates the entries whose keys are still alive.</summary>
    /// <returns>An enumerator over the entries.</returns>
#if v1_6_OR_GREATER
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        ((IEnumerable<KeyValuePair<TKey, TValue>>)innerConditionalWeakTable).GetEnumerator();
#else
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
#endif

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
