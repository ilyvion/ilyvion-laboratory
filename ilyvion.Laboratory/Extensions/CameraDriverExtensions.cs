using HarmonyLib;

namespace ilyvion.Laboratory.Extensions;

public static class CameraDriverExtensions
{
#if !v1_3
    private static AccessTools.FieldRef<CameraDriver, CameraPanner> CameraDriver_panner_ref = AccessTools.FieldRefAccess<CameraDriver, CameraPanner>("panner");
    public static bool IsPanning(this CameraDriver cameraDriver)
    {
        return CameraDriver_panner_ref(cameraDriver).Moving;
    }
#endif
}
