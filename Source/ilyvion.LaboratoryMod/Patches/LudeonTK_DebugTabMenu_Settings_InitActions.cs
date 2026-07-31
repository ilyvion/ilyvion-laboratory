#if !v1_3
#if !v1_4
using LudeonTK;
#endif
using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch(typeof(DebugTabMenu_Settings), nameof(DebugTabMenu_Settings.InitActions))]
internal static class LudeonTK_DebugTabMenu_Settings_InitActions
{
    private static void Postfix(DebugTabMenu_Settings __instance)
    {
        var fields = typeof(IlyvionDebugViewSettings).GetFields(AccessTools.all);

        var addNodeMethod = Traverse
            .Create(__instance)
            .Method("AddNode", paramTypes: [typeof(FieldInfo), typeof(string)]);
        foreach (var fi in fields)
        {
            _ = addNodeMethod.GetValue(fi, IlyvionDebugActionAttribute.IlyvionLaboratoryCategory);
        }
    }
}
#endif
