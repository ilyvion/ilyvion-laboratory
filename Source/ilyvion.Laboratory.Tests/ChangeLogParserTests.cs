// ChangeLogParserTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class ChangeLogParserTests
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

        Expect.IsFalse(entries.Select(entry => entry.Key).ToList().Contains("Test:Unreleased"));
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

        Expect.AreEqual(new DateTime(2024, 3, 14), entry.Date);
        Expect.AreEqual(2, entry.ContentList.Count);
        Expect.AreEqual("Added", entry.ContentList[0].header);
        Expect.AreEqual("- First added thing.\n- Second added thing.", entry.ContentList[0].text);
        Expect.AreEqual("Fixed", entry.ContentList[1].header);
        Expect.AreEqual("- A bug.", entry.ContentList[1].text);
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

        Expect.AreEqual(new DateTime(2023, 1, 1), entry.Date);
        Expect.AreEqual(1, entry.ContentList.Count);
        Expect.IsNull(entry.ContentList[0].header);
        Expect.AreEqual("- Initial release, no subsections.", entry.ContentList[0].text);
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

        Expect.AreEqual(1, entry.ContentList.Count);
        Expect.AreEqual(
            "- ilyvion's Laboratory: v0.13\n> [!IMPORTANT]\n> This release requires an update to ilyvion's Laboratory!",
            entry.ContentList[0].text
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

        Expect.IsNotEmpty(entries);
        foreach (var entry in entries)
        {
            Expect.AreEqual("Test mod", entry.SourceModName);
            Expect.AreEqual("Banner.png", entry.Banner);
            Expect.AreEqual("https://example.com", entry.LinkUrl);
            Expect.IsTrue(entry.Important);
        }
    }
}
