using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod.TabulaRasa;

/// <summary>
/// The interop mod class enabling the Tabula Rasa integration for ilyvion's Laboratory. Only ever
/// loads when Tabula Rasa is active, via <c>LoadFolders.xml</c>'s <c>IfModActive</c> gating - this
/// mod never needs to check for Tabula Rasa's presence itself.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Class is instantiated via reflection"
)]
internal sealed class TabulaRasaIntegrationMod : Mod
{
    public TabulaRasaIntegrationMod(ModContentPack content)
        : base(content)
    {
        new Harmony("ilyvion.LaboratoryMod.TabulaRasa").PatchAll(Assembly.GetExecutingAssembly());

        UpdateNotifications.RegisterUpdateSource(TabulaRasaUpdateAdapter.GetEntries);
    }
}
