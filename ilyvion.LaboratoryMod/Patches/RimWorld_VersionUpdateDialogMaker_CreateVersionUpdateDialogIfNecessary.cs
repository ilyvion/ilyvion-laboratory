using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch(
    typeof(VersionUpdateDialogMaker),
    nameof(VersionUpdateDialogMaker.CreateVersionUpdateDialogIfNecessary))]
internal static class RimWorld_VersionUpdateDialogMaker_CreateVersionUpdateDialogIfNecessary
{
    static void Postfix()
    {
        var requiredVersionRequests = VersionCheck.RequiredVersionRequests;

        foreach (var item in DefDatabase<VersionCheckDef>.AllDefs)
        {
            if (item.majorVersion == -1 || item.minorVersion == -1 || item.modName == null)
            {
                continue;
            }

            var requiredVersion = new Version(item.majorVersion, item.minorVersion);

            requiredVersionRequests ??= [];
            if (!VersionCheck.IsAtLeastVersion(requiredVersion))
            {
                requiredVersionRequests.Add(item.modName, requiredVersion);
            }
        }

        if (requiredVersionRequests == null)
        {
            return;
        }
        VersionCheck.RequiredVersionRequests = null;

        if (requiredVersionRequests.Count == 0)
        {
            return;
        }

        var modRequirements = string.Join("\n\t- ", requiredVersionRequests.Select(r =>
            $"{r.Key} requires at least version {r.Value}"));

        Find.WindowStack.Add(new Dialog_MessageBox(
            $"Some mod(s) have indicated that they require a newer version of " +
            $"ilyvion's Laboratory than you are currently running. You currently have version " +
            $"{VersionCheck.OurVersion.ToString(3)}. The mod(s) that required newer versions " +
            $"are: \n\t- {modRequirements}\n\n" +
            "If you use Steam, you can force an update by exiting the game, and unsubscribe and " +
            "resubscribe to the mod on the workshop. If you're manually installing the mod, you " +
            "can find the latest version on the mod's GitHub's releases page.\n\nYou will most " +
            "likely run into errors if you keep playing with an older version than the dependant " +
            "mods require."
        ));
    }
}
