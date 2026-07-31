using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ilyvion.Laboratory;

public static class CodeInstructionsExtensions
{
    public static bool CallMatches(this CodeInstruction code, Predicate<MethodInfo> methodPredicate)
    {
        return methodPredicate is null
            ? throw new ArgumentNullException(nameof(methodPredicate))
            : (
                (code.opcode == OpCodes.Call || code.opcode == OpCodes.Callvirt)
                && code.operand is MethodInfo methodInfo
                && methodPredicate(methodInfo)
            );
    }
}
