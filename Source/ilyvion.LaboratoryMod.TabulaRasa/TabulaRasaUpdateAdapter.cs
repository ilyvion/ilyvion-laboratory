using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod.TabulaRasa;

/// <summary>
/// Converts Tabula Rasa's own <see cref="global::TabulaRasa.UpdateDef"/>s into
/// <see cref="UpdateEntry"/>s, so they render through our own update-notifications popup instead
/// of Tabula Rasa's (which <see cref="Patch_UpdateUtil_DoUpdateListing"/> disables).
/// </summary>
internal static class TabulaRasaUpdateAdapter
{
    private const string KeyPrefix = "TabulaRasa:";

#if v1_3
    // Tabula Rasa's 1.3 release predates its contentList/UpdateItem/UpdateLink "new style"
    // update entries - only the legacy plain-text `content` field exists on this version.
    public static IEnumerable<UpdateEntry> GetEntries() =>
        DefDatabase<global::TabulaRasa.UpdateDef>
            .AllDefsListForReading.Where(def => !def.content.NullOrEmpty())
            .Select(ToUpdateEntry)
            .Where(entry => entry != null)
            .Cast<UpdateEntry>();

    private static UpdateEntry? ToUpdateEntry(global::TabulaRasa.UpdateDef def)
    {
        var parsed = DateTime.TryParseExact(
            def.date,
            "yyyy/M/d",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date
        );

        return !parsed
            ? null
            : new UpdateEntry(
                KeyPrefix + def.defName,
                def.modContentPack.Name,
                date,
                def.banner,
                [new UpdateContentItem { text = def.content }],
                null,
                def.linkUrl,
                def.important
            );
    }
#else
    public static IEnumerable<UpdateEntry> GetEntries() =>
        DefDatabase<global::TabulaRasa.UpdateDef>
            .AllDefsListForReading.Where(def =>
                !def.content.NullOrEmpty() || !def.contentList.NullOrEmpty()
            )
            .Select(ToUpdateEntry)
            .Where(entry => entry != null)
            .Cast<UpdateEntry>();

    private static UpdateEntry? ToUpdateEntry(global::TabulaRasa.UpdateDef def)
    {
        var parsed = DateTime.TryParseExact(
            def.date,
            "yyyy/M/d",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date
        );

        if (!parsed)
        {
            return null;
        }

        List<UpdateContentItem> contentList = def.content.NullOrEmpty()
            ? []
            : [new UpdateContentItem { text = def.content }];
        if (def.contentList != null)
        {
            contentList.AddRange(def.contentList.Select(ToContentItem));
        }

        return new UpdateEntry(
            KeyPrefix + def.defName,
            def.modContentPack.Name,
            date,
            def.banner,
            contentList,
            def.links?.Select(ToLink).ToList(),
            def.linkUrl,
            def.important
        );
    }

    private static UpdateContentItem ToContentItem(global::TabulaRasa.UpdateItem item) =>
        new()
        {
            header = item.header,
            image = item.image,
            text = item.text,
        };

    private static UpdateLink ToLink(global::TabulaRasa.UpdateLink link) =>
        new()
        {
            linkLabel = link.linkLabel,
            linkUrl = link.linkUrl,
            linkTex = link.linkTex,
        };
#endif
}
