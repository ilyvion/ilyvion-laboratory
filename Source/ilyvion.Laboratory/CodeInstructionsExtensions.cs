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

    /// <summary>
    /// Whether <paramref name="code"/> is a <c>newobj</c> or <c>call</c> of
    /// <paramref name="constructor"/>.
    /// </summary>
    [SinceVersion(0, 27, 0)]
    public static bool CallsConstructor(this CodeInstruction code, ConstructorInfo constructor)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(constructor);

        return code.CallsConstructorMatching(c => c == constructor);
    }

    /// <summary>
    /// Whether <paramref name="code"/> is a <c>newobj</c> or <c>call</c> of any constructor of
    /// <paramref name="declaringType"/>.
    /// </summary>
    [SinceVersion(0, 27, 0)]
    public static bool CallsConstructorOf(this CodeInstruction code, Type declaringType)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(declaringType);

        return code.CallsConstructorMatching(c => c.DeclaringType == declaringType);
    }

    private static bool CallsConstructorMatching(
        this CodeInstruction code,
        Predicate<ConstructorInfo> predicate
    ) =>
        (code.opcode == OpCodes.Newobj || code.opcode == OpCodes.Call)
        && code.operand is ConstructorInfo constructor
        && predicate(constructor);
}
