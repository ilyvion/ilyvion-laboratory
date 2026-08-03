namespace ilyvion.Laboratory;

public class VersionCheckDef : Def
{
    public int majorVersion = -1;
    public int minorVersion = -1;

    /// <summary>
    /// The build (third) component of the required version. Leave unset to only require a
    /// specific major/minor version.
    /// </summary>
    [SinceVersion(0, 23, 0)]
    public int? buildVersion;

    /// <summary>
    /// The revision (fourth) component of the required version. Only used when
    /// <see cref="buildVersion"/> is also set.
    /// </summary>
    [SinceVersion(0, 23, 0)]
    public int? revisionVersion;

    public string? modName;

    /// <summary>
    /// The package ID of the mod whose version this def checks. Defaults to
    /// <see cref="VersionCheck.OurModId"/> (ilyvion's Laboratory itself).
    /// </summary>
    [SinceVersion(0, 23, 0)]
    public string modId = VersionCheck.OurModId;

    /// <summary>
    /// The minimum version required, built from <see cref="majorVersion"/>,
    /// <see cref="minorVersion"/>, and, if set, <see cref="buildVersion"/> and
    /// <see cref="revisionVersion"/>.
    /// </summary>
    internal Version RequiredVersion =>
        buildVersion is { } build
            ? revisionVersion is { } revision
                ? new Version(majorVersion, minorVersion, build, revision)
                : new Version(majorVersion, minorVersion, build)
            : new Version(majorVersion, minorVersion);

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var item in base.ConfigErrors())
        {
            yield return item;
        }

        if (majorVersion == -1)
        {
            yield return $"{nameof(majorVersion)} is not set";
        }

        if (minorVersion == -1)
        {
            yield return $"{nameof(minorVersion)} is not set";
        }

        if (revisionVersion.HasValue && !buildVersion.HasValue)
        {
            yield return $"{nameof(revisionVersion)} is set without {nameof(buildVersion)}; "
                + $"{nameof(revisionVersion)} is ignored unless {nameof(buildVersion)} is also set";
        }

        if (modName == null)
        {
            yield return $"{nameof(modName)} is not set";
        }

        if (string.IsNullOrEmpty(modId))
        {
            yield return $"{nameof(modId)} is not set";
        }
    }
}
