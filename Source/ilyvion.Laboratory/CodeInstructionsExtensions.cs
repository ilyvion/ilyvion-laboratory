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

    private static readonly Dictionary<OpCode, int> ShortFormLocalIndices = new()
    {
        [OpCodes.Ldloc_0] = 0,
        [OpCodes.Ldloc_1] = 1,
        [OpCodes.Ldloc_2] = 2,
        [OpCodes.Ldloc_3] = 3,
        [OpCodes.Stloc_0] = 0,
        [OpCodes.Stloc_1] = 1,
        [OpCodes.Stloc_2] = 2,
        [OpCodes.Stloc_3] = 3,
    };

    private static readonly HashSet<OpCode> OperandLocalOpCodes =
    [
        OpCodes.Ldloc_S,
        OpCodes.Ldloc,
        OpCodes.Ldloca_S,
        OpCodes.Ldloca,
        OpCodes.Stloc_S,
        OpCodes.Stloc,
    ];

    /// <summary>
    /// The index of the local variable that <paramref name="code"/> loads, loads the address of
    /// or stores (any form of <c>ldloc</c>, <c>ldloca</c> or <c>stloc</c>).
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="code"/> does not access a local variable.
    /// </exception>
    [SinceVersion(0, 28, 0)]
    public static int LocalVariableIndex(this CodeInstruction code)
    {
        ArgumentNullException.ThrowIfNull(code);

        return ShortFormLocalIndices.TryGetValue(code.opcode, out var shortFormIndex)
                ? shortFormIndex
            : OperandLocalOpCodes.Contains(code.opcode)
                ? code.operand switch
                {
                    LocalBuilder local => local.LocalIndex,
                    byte index => index,
                    sbyte index => index,
                    short index => index,
                    ushort index => index,
                    int index => index,
                    _ => throw new ArgumentException(
                        $"{code} has an operand that is not a local variable.",
                        nameof(code)
                    ),
                }
            : throw new ArgumentException(
                $"{code} does not access a local variable.",
                nameof(code)
            );
    }

    /// <summary>
    /// Whether <paramref name="code"/> loads the value (not the address) of the local variable at
    /// <paramref name="index"/>, in any form of <c>ldloc</c>.
    /// </summary>
    [SinceVersion(0, 28, 0)]
    public static bool LoadsLocal(this CodeInstruction code, int index)
    {
        ArgumentNullException.ThrowIfNull(code);

        return code.IsLdloc()
            && code.opcode != OpCodes.Ldloca
            && code.opcode != OpCodes.Ldloca_S
            && code.LocalVariableIndex() == index;
    }

    /// <summary>
    /// A new instruction that loads the local variable <paramref name="code"/> accesses (see
    /// <see cref="LocalVariableIndex"/>).
    /// </summary>
    /// <inheritdoc cref="LocalVariableIndex" path="/exception"/>
    [SinceVersion(0, 28, 0)]
    public static CodeInstruction LoadSameLocal(this CodeInstruction code) =>
        code.LocalVariableIndex() switch
        {
            0 => new CodeInstruction(OpCodes.Ldloc_0),
            1 => new CodeInstruction(OpCodes.Ldloc_1),
            2 => new CodeInstruction(OpCodes.Ldloc_2),
            3 => new CodeInstruction(OpCodes.Ldloc_3),
            < 256 and var index => new CodeInstruction(OpCodes.Ldloc_S, (byte)index),
            var index => new CodeInstruction(OpCodes.Ldloc, index),
        };

    /// <summary>
    /// A new instruction that stores into the local variable <paramref name="code"/> accesses
    /// (see <see cref="LocalVariableIndex"/>).
    /// </summary>
    /// <inheritdoc cref="LocalVariableIndex" path="/exception"/>
    [SinceVersion(0, 28, 0)]
    public static CodeInstruction StoreSameLocal(this CodeInstruction code) =>
        code.LocalVariableIndex() switch
        {
            0 => new CodeInstruction(OpCodes.Stloc_0),
            1 => new CodeInstruction(OpCodes.Stloc_1),
            2 => new CodeInstruction(OpCodes.Stloc_2),
            3 => new CodeInstruction(OpCodes.Stloc_3),
            < 256 and var index => new CodeInstruction(OpCodes.Stloc_S, (byte)index),
            var index => new CodeInstruction(OpCodes.Stloc, index),
        };

    private static bool CallsConstructorMatching(
        this CodeInstruction code,
        Predicate<ConstructorInfo> predicate
    ) =>
        (code.opcode == OpCodes.Newobj || code.opcode == OpCodes.Call)
        && code.operand is ConstructorInfo constructor
        && predicate(constructor);
}
