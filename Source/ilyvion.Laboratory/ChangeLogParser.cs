using System.Text.RegularExpressions;

namespace ilyvion.Laboratory;

/// <summary>
/// Parses a Keep-a-Changelog-formatted (<see href="https://keepachangelog.com/en/1.0.0/"/>)
/// CHANGELOG.md into <see cref="UpdateEntry"/> values, one per released version section. Used by
/// <see cref="UpdateDef"/>s in <see cref="UpdateMode.ChangeLog"/> mode.
/// </summary>
internal static class ChangeLogParser
{
    private static readonly Regex VersionHeaderPattern = new(
        @"^##\s*\[(?<version>[^\]]+)\]\s*-?\s*(?<date>\d{4}-\d{2}-\d{2})?\s*$",
        RegexOptions.Compiled
    );

    private static readonly Regex SubsectionHeaderPattern = new(
        @"^###\s+(?<header>.+?)\s*$",
        RegexOptions.Compiled
    );

    private static readonly Regex BulletPattern = new(
        @"^\s*-\s+(?<text>.+?)\s*$",
        RegexOptions.Compiled
    );

    // Indented lines following a bullet (e.g. a GitHub-style "> [!IMPORTANT]" admonition nested
    // under it) are folded into that bullet's text rather than dropped, so MarkdownLite can render
    // them.
    private static readonly Regex ContinuationPattern = new(
        @"^\s+(?<text>\S.*?)\s*$",
        RegexOptions.Compiled
    );

    public static List<UpdateEntry> Parse(
        string changeLogText,
        string keyPrefix,
        string sourceModName,
        string? banner,
        string? linkUrl,
        List<UpdateLink>? links,
        bool important
    )
    {
        var entries = new List<UpdateEntry>();
        var lines = Regex.Split(changeLogText, "\r\n|\r|\n");

        var i = 0;
        while (i < lines.Length)
        {
            var headerMatch = VersionHeaderPattern.Match(lines[i]);
            if (!headerMatch.Success)
            {
                i++;
                continue;
            }

            var version = headerMatch.Groups["version"].Value;
            var dateGroup = headerMatch.Groups["date"];
            i++;

            var sectionStart = i;
            while (i < lines.Length && !VersionHeaderPattern.IsMatch(lines[i]))
            {
                i++;
            }

            // Sections without a parseable date (e.g. "[Unreleased]") aren't shown as updates.
            if (!dateGroup.Success)
            {
                continue;
            }

            var date = DateTime.ParseExact(
                dateGroup.Value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            );

            var contentList = ParseSection(lines, sectionStart, i);
            if (contentList.Count == 0)
            {
                continue;
            }

            entries.Add(
                new UpdateEntry(
                    keyPrefix + version,
                    sourceModName,
                    date,
                    banner,
                    contentList,
                    links,
                    linkUrl,
                    important
                )
            );
        }

        return entries;
    }

    private static List<UpdateContentItem> ParseSection(string[] lines, int start, int end)
    {
        var contentList = new List<UpdateContentItem>();
        string? currentHeader = null;
        List<string> currentBullets = [];

        void Flush()
        {
            if (currentHeader == null && currentBullets.Count == 0)
            {
                return;
            }

            contentList.Add(
                new UpdateContentItem
                {
                    header = currentHeader,
                    text =
                        currentBullets.Count == 0
                            ? null
                            : string.Join("\n", currentBullets.Select(bullet => "- " + bullet)),
                }
            );
            currentBullets = [];
        }

        for (var lineIndex = start; lineIndex < end; lineIndex++)
        {
            var line = lines[lineIndex];
            var subsectionMatch = SubsectionHeaderPattern.Match(line);
            if (subsectionMatch.Success)
            {
                Flush();
                currentHeader = subsectionMatch.Groups["header"].Value;
                continue;
            }

            var bulletMatch = BulletPattern.Match(line);
            if (bulletMatch.Success)
            {
                currentBullets.Add(bulletMatch.Groups["text"].Value);
                continue;
            }

            if (currentBullets.Count == 0)
            {
                continue;
            }

            var continuationMatch = ContinuationPattern.Match(line);
            if (continuationMatch.Success)
            {
                currentBullets[^1] += "\n" + continuationMatch.Groups["text"].Value;
            }
        }
        Flush();

        return contentList;
    }
}
