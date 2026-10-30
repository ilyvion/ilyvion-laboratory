// ScribeFileUtilityTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class ScribeFileUtilityTests
{
    private static string NewTempPath() =>
        Path.Combine(Path.GetTempPath(), $"ilyvion-scribe-{Guid.NewGuid():N}.xml");

    [Test]
    public static void RoundTripsValuesThroughFile()
    {
        var path = NewTempPath();
        try
        {
            var values = new List<int> { 3, 1, 4, 1, 5 };
            ScribeFileUtility.Save(
                path,
                "TestRoot",
                () => Scribe_Collections.Look(ref values, "values", LookMode.Value)
            );

            List<int> loaded = [];
            ScribeFileUtility.Load(
                path,
                () => Scribe_Collections.Look(ref loaded, "values", LookMode.Value)
            );

            Expect.AreEqual(5, loaded.Count);
            Expect.AreEqual(4, loaded[2]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public static void SaveWritesGivenRootNode()
    {
        var path = NewTempPath();
        try
        {
            ScribeFileUtility.Save(path, "TestRoot", () => { });

            Expect.IsTrue(File.ReadAllText(path).Contains("<TestRoot>", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public static void SaveFinalizesAndPropagatesWhenLookThrows()
    {
        var path = NewTempPath();
        try
        {
            var threw = false;
            try
            {
                ScribeFileUtility.Save(
                    path,
                    "TestRoot",
                    () => throw new InvalidOperationException("boom")
                );
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }

            Expect.IsTrue(threw);
            // Scribe is idle again, so a following save succeeds.
            ScribeFileUtility.Save(path, "TestRoot", () => { });
            Expect.IsTrue(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public static void LoadStopsScribeAndPropagatesWhenLookThrows()
    {
        var path = NewTempPath();
        try
        {
            ScribeFileUtility.Save(path, "TestRoot", () => { });

            var threw = false;
            try
            {
                ScribeFileUtility.Load(path, () => throw new InvalidOperationException("boom"));
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }

            Expect.IsTrue(threw);
            Expect.AreEqual(LoadSaveMode.Inactive, Scribe.mode);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
