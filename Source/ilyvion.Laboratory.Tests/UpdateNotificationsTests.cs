// UpdateNotificationsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class UpdateNotificationsTests
{
    [Test]
    public static void ConfigErrorsFlagsMissingDate()
    {
        var def = new UpdateDef { defName = "Test" };

        var errors = def.ConfigErrors().ToList();

        Expect.IsNotEmpty(errors);
        Expect.IsTrue(def.hasErrors);
    }

    [Test]
    public static void ConfigErrorsFlagsUnparseableDate()
    {
        var def = new UpdateDef { defName = "Test", date = "not-a-date" };

        var errors = def.ConfigErrors().ToList();

        Expect.IsNotEmpty(errors);
        Expect.IsTrue(def.hasErrors);
    }

    [Test]
    public static void ConfigErrorsAcceptsValidDate()
    {
        var def = new UpdateDef { defName = "Test", date = "2024-03-14" };

        var errors = def.ConfigErrors().ToList();

        Expect.IsEmpty(errors);
        Expect.IsFalse(def.hasErrors);
    }

    [Test]
    public static void ConfigErrorsSkipsDateValidationInChangeLogMode()
    {
        var def = new UpdateDef { defName = "Test", mode = UpdateMode.ChangeLog };

        var errors = def.ConfigErrors().ToList();

        Expect.IsEmpty(errors);
        Expect.IsFalse(def.hasErrors);
    }

    private static UpdateEntry MakeEntry(string key, DateTime date, bool withContent = true) =>
        new(
            key,
            "Test mod",
            date,
            null,
            withContent ? [new UpdateContentItem { text = "Something happened" }] : [],
            null,
            null,
            false
        );

    [Test]
    public static void ComputeAllUpdatesFiltersOutEntriesMarkedAsSeen()
    {
        var seen = MakeEntry("seen", new DateTime(2024, 1, 1));
        var unseen = MakeEntry("unseen", new DateTime(2024, 1, 2));

        var result = UpdateNotifications.ComputeAllUpdates(
            [],
            [seen, unseen],
            ["seen"],
            DateTime.MinValue
        );

        Expect.AreEqual(1, result.Count);
        Expect.AreEqual("unseen", result[0].Key);
    }

    [Test]
    public static void ComputeAllUpdatesFiltersOutEntriesOlderThanCutoff()
    {
        var old = MakeEntry("old", new DateTime(2020, 1, 1));
        var recent = MakeEntry("recent", new DateTime(2024, 1, 1));

        var result = UpdateNotifications.ComputeAllUpdates(
            [],
            [old, recent],
            [],
            new DateTime(2023, 1, 1)
        );

        Expect.AreEqual(1, result.Count);
        Expect.AreEqual("recent", result[0].Key);
    }

    [Test]
    public static void ComputeAllUpdatesFiltersOutEntriesWithNoContent()
    {
        var empty = MakeEntry("empty", new DateTime(2024, 1, 1), withContent: false);
        var withContent = MakeEntry("withContent", new DateTime(2024, 1, 2));

        var result = UpdateNotifications.ComputeAllUpdates(
            [],
            [empty, withContent],
            [],
            DateTime.MinValue
        );

        Expect.AreEqual(1, result.Count);
        Expect.AreEqual("withContent", result[0].Key);
    }

    [Test]
    public static void ComputeAllUpdatesSortsByDateDescending()
    {
        var oldest = MakeEntry("oldest", new DateTime(2022, 1, 1));
        var newest = MakeEntry("newest", new DateTime(2024, 1, 1));
        var middle = MakeEntry("middle", new DateTime(2023, 1, 1));

        var result = UpdateNotifications.ComputeAllUpdates(
            [],
            [oldest, newest, middle],
            [],
            DateTime.MinValue
        );

        Expect.IsTrue(result.Select(entry => entry.Key).ToList().Contains("newest"));
        Expect.AreEqual("newest", result[0].Key);
        Expect.AreEqual("middle", result[1].Key);
        Expect.AreEqual("oldest", result[2].Key);
    }
}
