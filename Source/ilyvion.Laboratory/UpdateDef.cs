namespace ilyvion.Laboratory;

/// <summary>
/// Describes a single "what's new" entry shown in the main menu's update-notifications popup.
/// Any mod that depends on ilyvion's Laboratory can define one of these to announce updates to
/// its players.
/// </summary>
[SinceVersion(0, 23, 0)]
public class UpdateDef : Def
{
    /// <summary>
    /// Whether this def's content is authored directly on it (<see cref="UpdateMode.Regular"/>,
    /// the default) or generated from a Keep-a-Changelog-formatted file
    /// (<see cref="UpdateMode.ChangeLog"/>).
    /// </summary>
    public UpdateMode mode = UpdateMode.Regular;

    /// <summary>
    /// Only used in <see cref="UpdateMode.ChangeLog"/> mode. Path to the changelog file, relative
    /// to the mod's root directory. Defaults to <c>CHANGELOG.md</c> if unset.
    /// </summary>
    public string? changeLogPath;

    /// <summary>
    /// Only used in <see cref="UpdateMode.Regular"/> mode. The date this update was published,
    /// formatted as <c>yyyy-MM-dd</c>. Used both for sorting and for display. Required; a missing
    /// or unparseable date is a config error and the def is excluded from the update-notifications
    /// popup entirely.
    /// </summary>
    public string? date;

    /// <summary>
    /// Banner image shown at the top of the update's details, should be 500x40.
    /// </summary>
    public string? banner;

    public List<UpdateContentItem> contentList = [];

    /// <summary>
    /// Link opened when clicking on the update. Shorthand for adding a <see cref="UpdateLink"/>
    /// with no label/icon to <see cref="links"/>.
    /// </summary>
    public string? linkUrl;

    public List<UpdateLink>? links;

    /// <summary>
    /// If true, makes the update stand out in the list. Only use for VITAL information, like a
    /// new dependency or major update.
    /// </summary>
    public bool important;

    /// <summary>
    /// Set by <see cref="ConfigErrors"/> when it yields an error, so callers can skip this def
    /// without having to re-derive its validity themselves.
    /// </summary>
    internal bool hasErrors;

    internal DateTime? ParsedDate { get; private set; }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var item in base.ConfigErrors())
        {
            yield return item;
        }

        if (mode == UpdateMode.ChangeLog)
        {
            yield break;
        }

        if (date == null)
        {
            hasErrors = true;
            yield return $"{nameof(date)} is not set";
            yield break;
        }

        if (
            !DateTime.TryParseExact(
                date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate
            )
        )
        {
            hasErrors = true;
            yield return $"{nameof(date)} '{date}' isn't a valid yyyy-MM-dd date";
            yield break;
        }

        ParsedDate = parsedDate;
    }
}

public enum UpdateMode
{
    Regular,
    ChangeLog,
}

public class UpdateContentItem
{
    public string? header;
    public string? image;
    public string? text;
}

public class UpdateLink
{
    public string? linkLabel;
    public string? linkUrl;
    public string? linkTex;
}
