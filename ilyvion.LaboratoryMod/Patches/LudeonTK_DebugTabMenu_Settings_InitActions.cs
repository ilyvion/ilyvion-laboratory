#if !v1_3
using System.Reflection;
#if !v1_4
using LudeonTK;
#endif
using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch(typeof(DebugTabMenu_Settings), nameof(DebugTabMenu_Settings.InitActions))]
internal static class LudeonTK_DebugTabMenu_Settings_InitActions
{
    static void Postfix(DebugTabMenu_Settings __instance)
    {
        FieldInfo[] fields = typeof(IlyvionDebugViewSettings).GetFields(AccessTools.all);

        var addNodeMethod = Traverse.Create(__instance).Method("AddNode", paramTypes: [typeof(FieldInfo), typeof(string)]);
        foreach (FieldInfo fi in fields)
        {
            addNodeMethod.GetValue(fi, "TaskList UI");
        }
    }
}
#endif
