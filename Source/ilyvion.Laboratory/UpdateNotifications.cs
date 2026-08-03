namespace ilyvion.Laboratory;

/// <summary>
/// Drives the main menu's update-notifications popup: a small "what's new" listing box in the
/// bottom-left corner, and an expanded details panel for whichever entry is selected. Ported from
/// Tabula Rasa's <c>UpdateUtil</c>, generalized to work off <see cref="UpdateEntry"/> so entries
/// can come from more than one source (see <see cref="RegisterUpdateSource"/>).
/// </summary>
[SinceVersion(0, 23, 0)]
public static class UpdateNotifications
{
    private static UpdateEntry? _selectedUpdate;
    private static Vector2 _updateScrollPosition;
    private static float _updateViewRectHeight;

    private static List<UpdateEntry>? AllUpdatesCached { get; set; }

    /// <summary>
    /// Whether the update-notifications popup should be shown at all. Config plumbing for the
    /// hosting mod's settings, which needs to pass this to <c>Scribe_Values.Look</c> by
    /// reference - not for other mods to poke at directly, hence internal rather than public.
    /// </summary>
    internal static bool ShowModUpdates = true;

    /// <summary>
    /// Keys (see <see cref="UpdateEntry.Key"/>) of entries the player has dismissed. Persisted by
    /// the hosting mod's settings.
    /// </summary>
    internal static List<string> MarkedAsSeen = [];

    /// <summary>
    /// Entries older than this are excluded from the popup. Set to "today" the first time this
    /// feature is ever loaded for a given install, so existing players aren't shown their entire
    /// update history at once; persisted by the hosting mod's settings from then on.
    /// </summary>
    internal static DateTime CutoffDate = DateTime.MinValue;

    /// <summary>
    /// Called after the player marks an entry as seen, so the hosting mod can persist
    /// <see cref="MarkedAsSeen"/>. Wired up by the hosting mod at startup.
    /// </summary>
    internal static Action? SaveSettings;

    private static readonly List<Func<IEnumerable<UpdateEntry>>> ExternalSources = [];

    /// <summary>
    /// Registers an additional source of update entries, e.g. an adapter that reads another mod's
    /// own update-announcement defs. Called once by an integration at startup.
    /// </summary>
    public static void RegisterUpdateSource(Func<IEnumerable<UpdateEntry>> source) =>
        ExternalSources.Add(source);

    public static List<UpdateEntry> AllUpdates =>
        AllUpdatesCached ??= ComputeAllUpdates(
            DefDatabase<UpdateDef>.AllDefsListForReading,
            ExternalSources.SelectMany(source => source()),
            MarkedAsSeen,
            CutoffDate
        );

    /// <summary>
    /// Pure merge/filter/sort step, kept separate from <see cref="AllUpdates"/> so it's testable
    /// without <c>DefDatabase</c>/game state.
    /// </summary>
    internal static List<UpdateEntry> ComputeAllUpdates(
        IEnumerable<UpdateDef> defs,
        IEnumerable<UpdateEntry> externalEntries,
        ICollection<string> markedAsSeen,
        DateTime cutoffDate
    )
    {
        var entries = defs.SelectMany(def => def.ToUpdateEntries()).Concat(externalEntries);

        var result = entries
            .Where(entry => !markedAsSeen.Contains(entry.Key))
            .Where(entry => entry.Date >= cutoffDate)
            .Where(entry => !entry.ContentList.NullOrEmpty())
            .OrderByDescending(entry => entry.Date)
            .ToList();

        return result;
    }

    public static void DoUpdateListing()
    {
        if (
            !ShowModUpdates
            || Verse.UI.screenHeight < 768
            || Verse.UI.screenWidth < 1366
            || AllUpdates.NullOrEmpty()
        )
        {
            return;
        }

        float height = 500;
        float width = 300;

        var rect = new Rect(8, Verse.UI.screenHeight - (height + 120), width, height);
        Widgets.DrawWindowBackground(rect);
        var inRect = rect.ContractedBy(16f);
        float curY = 0;
        var listing = new Listing_Standard();
        listing.Begin(inRect);

        _ = listing.Label("ilyvion.Laboratory.UpdateNotifications.ModUpdates".Translate());
        curY += Text.CalcHeight(
            "ilyvion.Laboratory.UpdateNotifications.ModUpdates".Translate(),
            inRect.width
        );

        listing.GapLine();
        curY += 12;

        var updateCount = Mathf.Min(6, AllUpdates.Count);

        for (var i = 0; i < updateCount; i++)
        {
            var listRect = new Rect(0, curY, inRect.width, 64);
            var hoverRect = new Rect(listRect);
            DoUpdateSelection(listRect, AllUpdates[i], Mouse.IsOver(hoverRect));
            curY += 68;
        }
        listing.End();

        MainMenuDrawer.DoExpansionIcons();

        if (_selectedUpdate != null)
        {
            DoSelectedUpdateInfo(_selectedUpdate);
        }
    }

    private static void RemoveSelection(string key)
    {
        MarkedAsSeen.Add(key);
        SaveSettings?.Invoke();
        InvalidateAllUpdatesCache();
    }

    /// <summary>
    /// Un-marks every entry the player has previously dismissed, so they show up in the popup
    /// again. Called from the hosting mod's settings window.
    /// </summary>
    internal static void UnmarkAllAsSeen()
    {
        MarkedAsSeen.Clear();
        InvalidateAllUpdatesCache();
    }

    /// <summary>
    /// Forces <see cref="AllUpdates"/> to recompute on next access.
    /// </summary>
    internal static void InvalidateAllUpdatesCache() => AllUpdatesCached = null;

    private static void DoUpdateSelection(Rect rect, UpdateEntry info, bool highlight = false)
    {
        if (info.Important)
        {
            Widgets.DrawWindowBackgroundTutor(rect);
        }
        else
        {
            Widgets.DrawWindowBackground(rect);
            if (highlight || _selectedUpdate == info)
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
        }
        if (Widgets.ButtonInvisible(rect))
        {
            _selectedUpdate = info;
        }
        var inRect = rect.ContractedBy(8);
        var listing = new Listing_Standard();

        listing.Begin(inRect);

        _ = listing.Label(info.SourceModName);
        _ = listing.Label(info.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        listing.End();
    }

    private static void DoSelectedUpdateInfo(UpdateEntry info)
    {
        float height = 500;
        float width = 500;

        var rect = new Rect(316, Verse.UI.screenHeight - (height + 120), width, height);
        Widgets.DrawWindowBackground(rect);
        var inRect = rect.ContractedBy(16);
        inRect.y += 12f;
        inRect.height -= 12f;
        if (CloseButtonFor(rect))
        {
            _selectedUpdate = null;
        }
        if (MarkAsReadButtonFor(rect))
        {
            var currentIndex = AllUpdates.IndexOf(info);
            RemoveSelection(info.Key);
            _selectedUpdate =
                currentIndex >= 0 && currentIndex < AllUpdates.Count
                    ? AllUpdates[currentIndex]
                    : null;
        }
        DoLinkIcons(rect, info);

        var flag = _updateViewRectHeight > inRect.height;
        var viewRect = new Rect(
            inRect.x,
            inRect.y,
            inRect.width - (flag ? 26f : 0f),
            _updateViewRectHeight
        );
        Widgets.BeginScrollView(inRect, ref _updateScrollPosition, viewRect);
        var listing = new Listing_Standard();
        var scrollRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 999999f);
        listing.Begin(scrollRect);

        if (!info.Banner.NullOrEmpty())
        {
            var bannerImage = ContentFinder<Texture2D>.Get(info.Banner, false);
            if (bannerImage != null)
            {
                listing.DoImage(bannerImage);
                listing.GapLine();
            }
        }
        DoUpdateContents(listing, info);

        _updateViewRectHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();
    }

    private static void DoLinkIcons(Rect rect, UpdateEntry info)
    {
        var links = info.Links ?? [];
        if (!info.LinkUrl.NullOrEmpty())
        {
            links = [.. links, new UpdateLink { linkUrl = info.LinkUrl }];
        }
        if (links.Count == 0)
        {
            return;
        }
        for (var i = 0; i < links.Count; i++)
        {
            var link = links[i];
            if (link.linkUrl.NullOrEmpty())
            {
                continue;
            }
            var icon = link.linkTex.NullOrEmpty()
                ? Resources.UpdateHyperlink
                : ContentFinder<Texture2D>.Get(link.linkTex, false);
            var iconRect = new Rect(rect.x + rect.width - (22f * (3 + i)), rect.y + 4, 18f, 18f);
            if (
                DoLinkButton(
                    iconRect,
                    icon,
                    link.linkLabel.NullOrEmpty() ? link.linkUrl : link.linkLabel
                )
            )
            {
                Application.OpenURL(link.linkUrl);
            }
        }
    }

    private static bool DoLinkButton(Rect rect, Texture2D? icon, string? tooltip = null)
    {
        if (tooltip != null)
        {
            TooltipHandler.TipRegion(rect, tooltip);
        }
        return icon != null && Widgets.ButtonImage(rect, icon);
    }

    private static void DoUpdateContents(Listing_Standard listing, UpdateEntry info)
    {
        foreach (var item in info.ContentList)
        {
            if (!item.header.NullOrEmpty())
            {
                listing.LabelBacked(item.header!, Color.white, GameFont.Small);
            }
            if (!item.text.NullOrEmpty())
            {
                listing.TextContent(item.text!);
            }
            if (!item.image.NullOrEmpty())
            {
                var image = ContentFinder<Texture2D>.Get(item.image, false);
                if (image != null)
                {
                    listing.DoImage(image);
                }
            }
        }
    }

    private static bool CloseButtonFor(Rect rect) =>
        DoLinkButton(
            new Rect(rect.x + rect.width - 18f - 4f, rect.y + 4, 18f, 18f),
            TexButton.CloseXSmall,
            "ilyvion.Laboratory.UpdateNotifications.Close".Translate()
        );

    private static bool MarkAsReadButtonFor(Rect rect) =>
        DoLinkButton(
            new Rect(rect.x + rect.width - 36f - 8f, rect.y + 4, 18f, 18f),
            Resources.UpdateMarkAsRead,
            "ilyvion.Laboratory.UpdateNotifications.MarkAsRead".Translate()
        );
}

internal static class ListingStandardUpdateExtensions
{
    // GitHub's admonition types (https://github.com/orgs/community/discussions/16925), each with
    // an icon/color echoing the colored side-bar GitHub itself renders. AdmonitionKind.None (a
    // plain blockquote with no marker) uses GitHub's own muted quote-bar gray and no icon/label.
    private static readonly Dictionary<
        AdmonitionKind,
        (Texture2D? Icon, string? Label, Color Color)
    > AdmonitionStyles = new()
    {
        [AdmonitionKind.None] = (null, null, new Color(0.545f, 0.576f, 0.62f)),
        [AdmonitionKind.Note] = (Resources.AdmonitionNote, "Note", new Color(0.345f, 0.651f, 1f)),
        [AdmonitionKind.Tip] = (Resources.AdmonitionTip, "Tip", new Color(0.247f, 0.725f, 0.314f)),
        [AdmonitionKind.Important] = (
            Resources.AdmonitionImportant,
            "Important",
            new Color(0.737f, 0.549f, 1f)
        ),
        [AdmonitionKind.Warning] = (
            Resources.AdmonitionWarning,
            "Warning",
            new Color(0.824f, 0.6f, 0.133f)
        ),
        [AdmonitionKind.Caution] = (
            Resources.AdmonitionCaution,
            "Caution",
            new Color(0.973f, 0.318f, 0.286f)
        ),
    };

    private const float BlockquoteBarWidth = 4f;
    private const float BlockquoteContentPadding = 8f;
    private const float BlockquoteIconSize = 16f;
    private const float BlockquoteIconGap = 6f;
    private const float BlockquoteTrailingGap = 8f;

    public static void TextContent(
        this Listing_Standard listing,
        string text,
        GameFont font = GameFont.Small
    )
    {
        Text.Font = font;
        foreach (var block in MarkdownLite.ToBlocks(text))
        {
            if (block.IsBlockquote)
            {
                listing.Blockquote(block.Admonition, block.RichText);
            }
            else
            {
                GUI.color = Color.white;
                _ = listing.Label(block.RichText);
            }
        }
        Text.Font = GameFont.Small;
    }

    private static void Blockquote(
        this Listing_Standard listing,
        AdmonitionKind kind,
        string bodyRichText
    )
    {
        var (icon, label, color) = AdmonitionStyles[kind];
        var hasHeader = label != null;

        var contentWidth =
            listing.ColumnWidth - BlockquoteBarWidth - (BlockquoteContentPadding * 2);
        var headerHeight = hasHeader ? BlockquoteIconSize + BlockquoteContentPadding : 0f;
        var bodyText = hasHeader ? bodyRichText : $"<i>{bodyRichText}</i>";
        var bodyHeight = Text.CalcHeight(bodyText, contentWidth);
        var totalHeight = headerHeight + bodyHeight + (BlockquoteContentPadding * 2);

        var outerRect = listing.GetRect(totalHeight).Rounded();

        GUI.color = new Color(color.r, color.g, color.b, 0.08f);
        GUI.DrawTexture(outerRect, BaseContent.WhiteTex);

        GUI.color = color;
        GUI.DrawTexture(
            new Rect(outerRect.x, outerRect.y, BlockquoteBarWidth, outerRect.height),
            BaseContent.WhiteTex
        );
        GUI.color = Color.white;

        var contentX = outerRect.x + BlockquoteBarWidth + BlockquoteContentPadding;
        var curY = outerRect.y + BlockquoteContentPadding;

        if (hasHeader)
        {
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelX = contentX;
            if (icon != null)
            {
                GUI.color = color;
                GUI.DrawTexture(
                    new Rect(contentX, curY, BlockquoteIconSize, BlockquoteIconSize),
                    icon
                );
                GUI.color = Color.white;
                labelX += BlockquoteIconSize + BlockquoteIconGap;
            }
            GUI.color = color;
            Widgets.Label(
                new Rect(labelX, curY, contentX + contentWidth - labelX, BlockquoteIconSize),
                $"<b>{label}</b>"
            );
            GUI.color = Color.white;
            Text.Anchor = anchor;
            curY += headerHeight;
        }

        Widgets.Label(new Rect(contentX, curY, contentWidth, bodyHeight), bodyText);

        listing.Gap(BlockquoteTrailingGap);
    }

    public static void LabelBacked(
        this Listing_Standard list,
        string inputText,
        Color color,
        GameFont font = GameFont.Medium
    )
    {
        inputText = MarkdownLite.ToRichText(inputText);

        Text.Font = font;
        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        var height = Text.CalcHeight(inputText, list.ColumnWidth - 3f - 6f) + 6f;
        var rect = list.GetRect(height).Rounded();
        var backgroundColor = color;
        backgroundColor.r *= 0.25f;
        backgroundColor.g *= 0.25f;
        backgroundColor.b *= 0.25f;
        backgroundColor.a *= 0.2f;
        GUI.color = backgroundColor;
        var position = rect.ContractedBy(1f);
        position.yMax -= 2f;
        GUI.DrawTexture(position, BaseContent.WhiteTex);
        GUI.color = color;
        rect.xMin += 6f;
        Widgets.Label(rect, inputText);
        GUI.color = Color.white;
        Text.Anchor = anchor;
        Text.Font = GameFont.Small;
    }

    public static void DoImage(this Listing_Standard listing, Texture2D image)
    {
        var rect = listing.GetRect(image.height).Rounded();
        GUI.DrawTexture(rect, image);
        listing.Gap(4f);
    }
}
