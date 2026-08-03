// ChangeLogParserTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class ChangeLogParserTests
{
    private const string SampleChangeLog = """
        # Changelog

        All notable changes to this project will be documented in this file.

        ## [Unreleased]

        ### Added

        - Something not yet released.

        ## [1.1.0] - 2024-03-14

        ### Added

        - First added thing.
        - Second added thing.

        ### Fixed

        - A bug.

        ## [1.0.0] 2023-01-01

        - Initial release, no subsections.

        ## [0.5.0] - 2024-09-15

        ### Dependencies

        - ilyvion's Laboratory: v0.13
            > [!IMPORTANT]
            > This release requires an update to ilyvion's Laboratory!
        """;

    [Test]
    public static void ParseSkipsSectionsWithoutADate()
    {
        var entries = ChangeLogParser.Parse(
            SampleChangeLog,
            "Test:",
            "Test mod",
            null,
            null,
            null,
            false
        );

        Assert
            .ThatCollection(entries.Select(entry => entry.Key).ToList())
            .Does.Not.Contain("Test:Unreleased");
    }

    [Test]
    public static void ParseGroupsBulletsUnderTheirSubsectionHeader()
    {
        var entries = ChangeLogParser.Parse(
            SampleChangeLog,
            "Test:",
            "Test mod",
            null,
            null,
            null,
            false
        );

        var entry = entries.Single(entry => entry.Key == "Test:1.1.0");

        Assert.That(entry.Date).Is.EqualTo(new DateTime(2024, 3, 14));
        Assert.ThatCollection(entry.ContentList).Has.Count(2);
        Assert.That(entry.ContentList[0].header).Is.EqualTo("Added");
        Assert
            .That(entry.ContentList[0].text)
            .Is.EqualTo("- First added thing.\n- Second added thing.");
        Assert.That(entry.ContentList[1].header).Is.EqualTo("Fixed");
        Assert.That(entry.ContentList[1].text).Is.EqualTo("- A bug.");
    }

    [Test]
    public static void ParseAcceptsAVersionHeaderWithNoDashBeforeTheDate()
    {
        var entries = ChangeLogParser.Parse(
            SampleChangeLog,
            "Test:",
            "Test mod",
            null,
            null,
            null,
            false
        );

        var entry = entries.Single(entry => entry.Key == "Test:1.0.0");

        Assert.That(entry.Date).Is.EqualTo(new DateTime(2023, 1, 1));
        Assert.ThatCollection(entry.ContentList).Has.Count(1);
        Assert.That(entry.ContentList[0].header).Is.Null();
        Assert.That(entry.ContentList[0].text).Is.EqualTo("- Initial release, no subsections.");
    }

    [Test]
    public static void ParseFoldsIndentedLinesFollowingABulletIntoThatBulletsText()
    {
        var entries = ChangeLogParser.Parse(
            SampleChangeLog,
            "Test:",
            "Test mod",
            null,
            null,
            null,
            false
        );

        var entry = entries.Single(entry => entry.Key == "Test:0.5.0");

        Assert.ThatCollection(entry.ContentList).Has.Count(1);
        Assert
            .That(entry.ContentList[0].text)
            .Is.EqualTo(
                "- ilyvion's Laboratory: v0.13\n> [!IMPORTANT]\n> This release requires an update to ilyvion's Laboratory!"
            );
    }

    [Test]
    public static void ParseCarriesTheSameSourceFieldsOntoEveryEntry()
    {
        var entries = ChangeLogParser.Parse(
            SampleChangeLog,
            "Test:",
            "Test mod",
            "Banner.png",
            "https://example.com",
            null,
            true
        );

        Assert.ThatCollection(entries).Is.Not.Empty();
        foreach (var entry in entries)
        {
            Assert.That(entry.SourceModName).Is.EqualTo("Test mod");
            Assert.That(entry.Banner).Is.EqualTo("Banner.png");
            Assert.That(entry.LinkUrl).Is.EqualTo("https://example.com");
            Assert.That(entry.Important).Is.True();
        }
    }
}
