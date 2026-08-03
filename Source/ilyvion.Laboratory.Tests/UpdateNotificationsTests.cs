// UpdateNotificationsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class UpdateNotificationsTests
{
    [Test]
    public static void ConfigErrorsFlagsMissingDate()
    {
        var def = new UpdateDef { defName = "Test" };

        var errors = def.ConfigErrors().ToList();

        Assert.ThatCollection(errors).Is.Not.Empty();
        Assert.That(def.hasErrors).Is.True();
    }

    [Test]
    public static void ConfigErrorsFlagsUnparseableDate()
    {
        var def = new UpdateDef { defName = "Test", date = "not-a-date" };

        var errors = def.ConfigErrors().ToList();

        Assert.ThatCollection(errors).Is.Not.Empty();
        Assert.That(def.hasErrors).Is.True();
    }

    [Test]
    public static void ConfigErrorsAcceptsValidDate()
    {
        var def = new UpdateDef { defName = "Test", date = "2024-03-14" };

        var errors = def.ConfigErrors().ToList();

        Assert.ThatCollection(errors).Is.Empty();
        Assert.That(def.hasErrors).Is.False();
    }

    [Test]
    public static void ConfigErrorsSkipsDateValidationInChangeLogMode()
    {
        var def = new UpdateDef { defName = "Test", mode = UpdateMode.ChangeLog };

        var errors = def.ConfigErrors().ToList();

        Assert.ThatCollection(errors).Is.Empty();
        Assert.That(def.hasErrors).Is.False();
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

        Assert.ThatCollection(result).Has.Count(1);
        Assert.That(result[0].Key).Is.EqualTo("unseen");
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

        Assert.ThatCollection(result).Has.Count(1);
        Assert.That(result[0].Key).Is.EqualTo("recent");
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

        Assert.ThatCollection(result).Has.Count(1);
        Assert.That(result[0].Key).Is.EqualTo("withContent");
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

        Assert.ThatCollection(result.Select(entry => entry.Key).ToList()).Does.Contain("newest");
        Assert.That(result[0].Key).Is.EqualTo("newest");
        Assert.That(result[1].Key).Is.EqualTo("middle");
        Assert.That(result[2].Key).Is.EqualTo("oldest");
    }
}
