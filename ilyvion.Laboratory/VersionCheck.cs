using System.Reflection;

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
            Action? cancelAction = null)
            : base(
                text,
                buttonAText,
                buttonAAction,
                buttonBText,
                buttonBAction,
                title,
                buttonADestructive,
                acceptAction,
                cancelAction)
        {
        }
    }

    internal static Dictionary<string, Version>? RequiredVersionRequests { get; set; } = [];

    public static bool IsAtLeastVersion(Version requiredVersion)
    {
        return OurVersion > requiredVersion;
    }

    public static Version OurVersion => Assembly.GetExecutingAssembly().GetName().Version;

    public static void ShowRequiresAtLeastVersionMessageFor(
        Version requiredVersion,
        string modName)
    {
        if (RequiredVersionRequests == null)
        {
            throw new InvalidOperationException("Calls to ShowRequiresAtLeastVersionMessageFor " +
                "may only be made before the game's main menu is shown for the first time");
        }
        if (!IsAtLeastVersion(requiredVersion))
        {
            RequiredVersionRequests.Add(modName, requiredVersion);
        }
    }
}
