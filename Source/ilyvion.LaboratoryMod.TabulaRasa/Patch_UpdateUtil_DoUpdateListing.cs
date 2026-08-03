namespace ilyvion.LaboratoryMod.TabulaRasa;

/// <summary>
/// Disables Tabula Rasa's own update-notifications popup outright - our own popup (see
/// <see cref="TabulaRasaIntegrationMod"/>/<see cref="TabulaRasaUpdateAdapter"/>) absorbs its
/// <c>UpdateDef</c>s instead, so both mods don't show competing UI.
/// </summary>
[HarmonyPatch(
    typeof(global::TabulaRasa.UpdateUtil),
    nameof(global::TabulaRasa.UpdateUtil.DoUpdateListing)
)]
internal static class Patch_UpdateUtil_DoUpdateListing
{
    private static bool Prefix() => false;
}
