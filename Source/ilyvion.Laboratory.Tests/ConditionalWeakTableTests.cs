// ConditionalWeakTableTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

#if !v1_6
using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class ConditionalWeakTableTests
{
    // GetOrCreateValue requires a public parameterless constructor at runtime.
    private sealed class Value { }

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
        var table = new ConditionalWeakTable<object, Value>();
        var key = new object();

        _ = table.GetOrCreateValue(key);
        _ = table.GetOrCreateValue(key);
        _ = table.GetOrCreateValue(key);

        Assert.That(KeyReferenceCount(table)).Is.EqualTo(1);
        Assert.ThatCollection(table).Has.Count(1);
    }

    [Test]
    public static void GetValueWithSameKeyDoesNotDuplicateReference()
    {
        var table = new ConditionalWeakTable<object, string>();
        var key = new object();

        _ = table.GetValue(key, _ => "value");
        _ = table.GetValue(key, _ => "value");

        Assert.That(KeyReferenceCount(table)).Is.EqualTo(1);
        Assert.ThatCollection(table).Has.Count(1);
    }

    [Test]
    public static void DeadKeyReferencesArePrunedOnNextTrackedOperation()
    {
        var table = new ConditionalWeakTable<object, string>();
        AddAndDropKey(table);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        table.Add(new object(), "value");

        Assert.That(KeyReferenceCount(table)).Is.EqualTo(1);
    }

    private static void AddAndDropKey(ConditionalWeakTable<object, string> table) =>
        table.Add(new object(), "value");
}
#endif
