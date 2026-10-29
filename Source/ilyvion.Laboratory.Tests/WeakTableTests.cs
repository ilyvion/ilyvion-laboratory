// WeakTableTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class WeakTableTests
{
    // GetOrCreateValue requires a public parameterless constructor at runtime.
    private sealed class Value { }

    [Test]
    public static void AddOrUpdateAddsAMissingKey()
    {
        var table = new WeakTable<object, string>();
        var key = new object();

        table.AddOrUpdate(key, "value");

        Expect.IsTrue(table.TryGetValue(key, out var value));
        Expect.AreEqual("value", value);
    }

    [Test]
    public static void AddOrUpdateReplacesAnExistingValue()
    {
        var table = new WeakTable<object, string>();
        var key = new object();
        table.Add(key, "old");

        table.AddOrUpdate(key, "new");

        Expect.IsTrue(table.TryGetValue(key, out var value));
        Expect.AreEqual("new", value);
        Expect.AreEqual(1, table.Count());
    }

    [Test]
    public static void ClearRemovesEveryEntry()
    {
        var table = new WeakTable<object, string>();
        var first = new object();
        var second = new object();
        table.Add(first, "first");
        _ = table.GetValue(second, _ => "second");

        table.Clear();

        Expect.IsFalse(table.TryGetValue(first, out _));
        Expect.IsFalse(table.TryGetValue(second, out _));
        Expect.AreEqual(0, table.Count());
    }

    [Test]
    public static void EnumerationYieldsEveryEntry()
    {
        var table = new WeakTable<object, string>();
        var first = new object();
        var second = new object();
        table.Add(first, "first");
        table.Add(second, "second");

        var entries = table.ToDictionary(entry => entry.Key, entry => entry.Value);

        Expect.AreEqual(2, entries.Count);
        Expect.AreEqual("first", entries[first]);
        Expect.AreEqual("second", entries[second]);
    }

    [Test]
    public static void RemoveDropsTheEntry()
    {
        var table = new WeakTable<object, string>();
        var key = new object();
        table.Add(key, "value");

        Expect.IsTrue(table.Remove(key));

        Expect.IsFalse(table.TryGetValue(key, out _));
        Expect.AreEqual(0, table.Count());
        Expect.IsFalse(table.Remove(key));
    }

#if !v1_6_OR_GREATER
    private static int KeyReferenceCount(object table)
    {
        var field = table
            .GetType()
            .GetField("keyReferencesByHash", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var bucketsByHash = (IDictionary)field.GetValue(table)!;
        var count = 0;
        foreach (ICollection bucket in bucketsByHash.Values)
        {
            count += bucket.Count;
        }
        return count;
    }

    // Regression guard for BUG-12: GetOrCreateValue/GetValue used to add a fresh WeakReference
    // on every call instead of deduping on the already-tracked live key, so repeated lookups of
    // the same key grew keyReferences without bound and could yield duplicate enumeration
    // entries.
    [Test]
    public static void GetOrCreateValueWithSameKeyDoesNotDuplicateReference()
    {
        var table = new WeakTable<object, Value>();
        var key = new object();

        _ = table.GetOrCreateValue(key);
        _ = table.GetOrCreateValue(key);
        _ = table.GetOrCreateValue(key);

        Expect.AreEqual(1, KeyReferenceCount(table));
        Expect.AreEqual(1, table.Count());
    }

    [Test]
    public static void GetValueWithSameKeyDoesNotDuplicateReference()
    {
        var table = new WeakTable<object, string>();
        var key = new object();

        _ = table.GetValue(key, _ => "value");
        _ = table.GetValue(key, _ => "value");

        Expect.AreEqual(1, KeyReferenceCount(table));
        Expect.AreEqual(1, table.Count());
    }

    [Test]
    public static void DeadKeyReferencesArePrunedOnNextTrackedOperation()
    {
        var table = new WeakTable<object, string>();
        AddAndDropKey(table);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        table.Add(new object(), "value");

        Expect.AreEqual(1, KeyReferenceCount(table));
    }

    private static void AddAndDropKey(WeakTable<object, string> table) =>
        table.Add(new object(), "value");
#else
    [Test]
    public static void GetOrCreateValueWithSameKeyReturnsTheSameValue()
    {
        var table = new WeakTable<object, Value>();
        var key = new object();

        var first = table.GetOrCreateValue(key);
        var second = table.GetOrCreateValue(key);

        Expect.ReferencesAreEqual(first, second);
        Expect.AreEqual(1, table.Count());
    }
#endif
}
