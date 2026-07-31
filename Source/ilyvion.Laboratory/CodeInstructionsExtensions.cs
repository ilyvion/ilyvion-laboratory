using System.Reflection.Emit;

namespace ilyvion.Laboratory;

public static class CodeInstructionsExtensions
{
    public static bool CallMatches(
        this CodeInstruction code,
        Predicate<MethodInfo> methodPredicate
    ) =>
        methodPredicate is null
            ? throw new ArgumentNullException(nameof(methodPredicate))
            : (
                code is null
                    ? throw new ArgumentNullException(nameof(code))
                    : (
                        (code.opcode == OpCodes.Call || code.opcode == OpCodes.Callvirt)
                        && code.operand is MethodInfo methodInfo
                        && methodPredicate(methodInfo)
                    )
            );
}
