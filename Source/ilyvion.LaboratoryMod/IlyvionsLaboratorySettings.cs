using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

internal sealed class IlyvionsLaboratorySettings : ModSettings
{
    internal bool HasInitializedUpdateCutoff;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref UpdateNotifications.ShowModUpdates, "showModUpdates", true);
        Scribe_Collections.Look(
            ref UpdateNotifications.MarkedAsSeen,
            "updateNotificationsMarkedAsSeen"
        );
        UpdateNotifications.MarkedAsSeen ??= [];

        // Verse's ParseHelper has no built-in DateTime parser, so Scribe_Values.Look can't
        // round-trip a DateTime directly - scribe its ticks instead.
        var cutoffDateTicks = UpdateNotifications.CutoffDate.Ticks;
        Scribe_Values.Look(ref cutoffDateTicks, "updateNotificationsCutoffDate", 0L);
        UpdateNotifications.CutoffDate = new DateTime(cutoffDateTicks);

        Scribe_Values.Look(ref HasInitializedUpdateCutoff, "hasInitializedUpdateCutoff", false);
    }

    internal static void DoWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.CheckboxLabeled(
            "ilyvion.Laboratory.UpdateNotifications.Settings.ShowModUpdates".Translate(),
            ref UpdateNotifications.ShowModUpdates
        );

        if (
            listing.ButtonText(
                "ilyvion.Laboratory.UpdateNotifications.Settings.ShowAllOldUpdates".Translate()
            )
        )
        {
            UpdateNotifications.CutoffDate = DateTime.MinValue;
            UpdateNotifications.InvalidateAllUpdatesCache();
        }

        if (
            listing.ButtonText(
                "ilyvion.Laboratory.UpdateNotifications.Settings.UnmarkAllAsSeen".Translate()
            )
        )
        {
            UpdateNotifications.UnmarkAllAsSeen();
        }

        listing.End();
    }
}
