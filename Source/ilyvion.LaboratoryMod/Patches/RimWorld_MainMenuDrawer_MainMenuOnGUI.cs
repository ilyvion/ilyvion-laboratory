using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.MainMenuOnGUI))]
internal static class RimWorld_MainMenuDrawer_MainMenuOnGUI
{
    private static void Postfix() => UpdateNotifications.DoUpdateListing();
}
