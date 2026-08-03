using System.Diagnostics.CodeAnalysis;
using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch(
    typeof(VersionUpdateDialogMaker),
    nameof(VersionUpdateDialogMaker.CreateVersionUpdateDialogIfNecessary)
)]
internal static class RimWorld_VersionUpdateDialogMaker_CreateVersionUpdateDialogIfNecessary
{
    // Verse.ModMetaData.ModVersion doesn't exist prior to 1.4, so under 1.3 only the def's own
    // mod (checked via VersionCheck.OurVersion) can be resolved.
    private static bool TryGetTargetModActualVersion(
        VersionCheckDef item,
        [NotNullWhen(true)] out string? targetModName,
        [NotNullWhen(true)] out Version? actualVersion
    )
    {
#if v1_3
        if (item.modId == VersionCheck.OurModId)
        {
            targetModName = "ilyvion's Laboratory";
            actualVersion = VersionCheck.OurVersion;
            return true;
        }

        Laboratory.Logger.LogWarning(
            $"VersionCheckDef '{item.defName}' (from '{item.modName}') checks the version of "
                + $"mod '{item.modId}', but checking the version of another mod isn't supported "
                + "on RimWorld 1.3; skipping this version check."
        );
        targetModName = null;
        actualVersion = null;
        return false;
#else
        var targetMod = ModLister.GetModWithIdentifier(item.modId, true);
        if (targetMod == null)
        {
            // Target mod isn't installed/active; nothing to check against.
            targetModName = null;
            actualVersion = null;
            return false;
        }

        if (!Version.TryParse(targetMod.ModVersion, out var parsedVersion))
        {
            Laboratory.Logger.LogWarning(
                $"VersionCheckDef '{item.defName}' (from '{item.modName}') checks the version "
                    + $"of mod '{item.modId}', but that mod's version ('{targetMod.ModVersion}') "
                    + "isn't a parseable version; skipping this version check."
            );
            targetModName = null;
            actualVersion = null;
            return false;
        }

        targetModName = targetMod.Name;
        actualVersion = parsedVersion;
        return true;
#endif
    }

    private static void Postfix()
    {
        var requiredVersionRequests = VersionCheck.RequiredVersionRequests;

        foreach (var item in DefDatabase<VersionCheckDef>.AllDefs)
        {
            if (item.majorVersion == -1 || item.minorVersion == -1 || item.modName == null)
            {
                continue;
            }

            if (!TryGetTargetModActualVersion(item, out var targetModName, out var actualVersion))
            {
                continue;
            }

            var requiredVersion = item.RequiredVersion;

            requiredVersionRequests ??= [];
            if (actualVersion < requiredVersion)
            {
                if (requiredVersionRequests.TryGetValue(item.modName, out var existing))
                {
                    Laboratory.Logger.LogWarning(
                        $"Multiple VersionCheckDefs present for mod '{item.modName}'; "
                            + "keeping the highest requested version."
                    );
                    if (existing.RequiredVersion < requiredVersion)
                    {
                        requiredVersionRequests[item.modName] = new VersionCheck.VersionRequirement(
                            item.modId,
                            targetModName,
                            requiredVersion,
                            actualVersion
                        );
                    }
                }
                else
                {
                    requiredVersionRequests.Add(
                        item.modName,
                        new VersionCheck.VersionRequirement(
                            item.modId,
                            targetModName,
                            requiredVersion,
                            actualVersion
                        )
                    );
                }
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

        var modRequirements = string.Join(
            "\n\t- ",
            requiredVersionRequests.Select(r =>
                $"{r.Key} requires {r.Value.TargetModName} to be at least version "
                + $"{r.Value.RequiredVersion}, but you have {r.Value.ActualVersion}"
            )
        );

        Find.WindowStack.Add(
            new Dialog_MessageBox(
                $"Some mod(s) have indicated that they require a newer version of another mod "
                    + $"than you are currently running. The mod(s) that required newer versions "
                    + $"are: \n\t- {modRequirements}\n\n"
                    + "If you use Steam, you can force an update by exiting the game, and unsubscribe and "
                    + "resubscribe to the affected mod(s) on the workshop. If you're manually installing "
                    + "the mod, you can find the latest version on the mod's GitHub's releases page.\n\n"
                    + "You will most likely run into errors if you keep playing with an older version "
                    + "than the dependant mods require."
            )
        );
    }
}
