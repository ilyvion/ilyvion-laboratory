namespace ilyvion.Laboratory;

public static class VersionCheck
{
    internal class VersionDialog : Dialog_MessageBox
    {
        public override Vector2 InitialSize => new(640f, 240f);

        internal VersionDialog(
            string text,
            string? buttonAText = null,
            Action? buttonAAction = null,
            string? buttonBText = null,
            Action? buttonBAction = null,
            string? title = null,
            bool buttonADestructive = false,
            Action? acceptAction = null,
            Action? cancelAction = null
        )
            : base(
                text,
                buttonAText,
                buttonAAction,
                buttonBText,
                buttonBAction,
                title,
                buttonADestructive,
                acceptAction,
                cancelAction
            ) { }
    }

    internal readonly record struct VersionRequirement(
        string TargetModId,
        string TargetModName,
        Version RequiredVersion,
        Version ActualVersion
    );

    internal static Dictionary<string, VersionRequirement>? RequiredVersionRequests { get; set; } =
    [];

    public static bool IsAtLeastVersion(Version requiredVersion) => OurVersion >= requiredVersion;

    public static Version OurVersion => Assembly.GetExecutingAssembly().GetName().Version;

    /// <summary>
    /// The package ID ilyvion's Laboratory itself is published under, i.e. the default a
    /// <see cref="VersionCheckDef"/> checks against when no other mod is specified.
    /// </summary>
    [SinceVersion(0, 23, 0)]
    public const string OurModId = "ilyvion.laboratory";
}
