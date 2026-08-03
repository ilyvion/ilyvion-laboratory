namespace ilyvion.Laboratory;

/// <summary>
/// The common display model the update-notifications popup renders. Every update source (our own
/// <see cref="UpdateDef"/>s, or defs adapted in from another mod, e.g. a Tabula Rasa integration)
/// converts to this shape, so the rendering/sorting/seen-tracking code in
/// <see cref="UpdateNotifications"/> never needs to know where an entry came from.
/// </summary>
/// <remarks>
/// <paramref name="Key"/> is the globally-unique identifier used for "mark as seen" tracking. Our
/// own defs use their <c>defName</c> verbatim; adapted entries from another mod should prefix it
/// (e.g. <c>"TabulaRasa:" + defName</c>) so the two namespaces can't collide.
/// </remarks>
#pragma warning disable CA1054, CA1056 // Uri parameters/properties should not be strings - LinkUrl mirrors UpdateDef's own string field and is only ever passed to Application.OpenURL(string)
[SinceVersion(0, 23, 0)]
public sealed record UpdateEntry(
    string Key,
    string SourceModName,
    DateTime Date,
    string? Banner,
    List<UpdateContentItem> ContentList,
    List<UpdateLink>? Links,
    string? LinkUrl,
    bool Important
);
#pragma warning restore CA1054, CA1056

internal static class UpdateDefExtensions
{
    /// <summary>
    /// Converts a def to its <see cref="UpdateEntry"/> values. Yields exactly one entry in
    /// <see cref="UpdateMode.Regular"/> mode, or one entry per released version section of the
    /// def's changelog file in <see cref="UpdateMode.ChangeLog"/> mode (none if that file doesn't
    /// exist).
    /// </summary>
    public static IEnumerable<UpdateEntry> ToUpdateEntries(this UpdateDef def)
    {
        if (def.hasErrors)
        {
            yield break;
        }

        if (def.mode == UpdateMode.ChangeLog)
        {
            var path = Path.Combine(
                def.modContentPack.RootDir,
                def.changeLogPath ?? "CHANGELOG.md"
            );
            if (!File.Exists(path))
            {
                yield break;
            }

            foreach (
                var entry in ChangeLogParser.Parse(
                    File.ReadAllText(path),
                    def.defName + ":",
                    def.modContentPack.Name,
                    def.banner,
                    def.linkUrl,
                    def.links,
                    def.important
                )
            )
            {
                yield return entry;
            }

            yield break;
        }

        if (def.ParsedDate is not { } date)
        {
            yield break;
        }

        yield return new UpdateEntry(
            def.defName,
            def.modContentPack.Name,
            date,
            def.banner,
            def.contentList,
            def.links,
            def.linkUrl,
            def.important
        );
    }
}
