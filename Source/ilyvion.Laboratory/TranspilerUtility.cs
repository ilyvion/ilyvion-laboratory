// TranspilerUtility.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.Reflection.Emit;

namespace ilyvion.Laboratory;

/// <summary>
/// Helpers for writing Harmony transpilers.
/// </summary>
[SinceVersion(0, 27, 0)]
public static class TranspilerUtility
{
    /// <summary>
    /// Inserts a call to <paramref name="methodToCall"/>, preceded by
    /// <paramref name="additionalInstructionsBeforeCall"/>, right before the last instruction
    /// matching <paramref name="matchInstruction"/> that precedes the method's final
    /// <c>ret</c>.
    /// </summary>
    /// <exception cref="InjectCallBeforeReturnException">
    /// The last instruction is not <c>ret</c>, or no instruction before it matches
    /// <paramref name="matchInstruction"/>.
    /// </exception>
    public static IEnumerable<CodeInstruction> InjectCallBeforeReturn(
        IEnumerable<CodeInstruction> instructions,
        MethodInfo methodToCall,
        Func<CodeInstruction, bool> matchInstruction,
        IEnumerable<CodeInstruction>? additionalInstructionsBeforeCall = null
    )
    {
        ArgumentNullException.ThrowIfNull(matchInstruction);

        var codeMatcher = new CodeMatcher(instructions);
        _ = codeMatcher.End();
        if (codeMatcher.Instruction.opcode != OpCodes.Ret)
        {
            throw new InjectCallBeforeReturnException(codeMatcher.Instructions());
        }
        while (codeMatcher.Pos > 0)
        {
            _ = codeMatcher.Advance(-1);
            if (matchInstruction(codeMatcher.Instruction))
            {
                _ = codeMatcher.Insert(
                    (additionalInstructionsBeforeCall ?? []).Concat([
                        new CodeInstruction(OpCodes.Call, methodToCall),
                    ])
                );

                return codeMatcher.Instructions();
            }
        }

        throw new InjectCallBeforeReturnException(codeMatcher.Instructions());
    }

    /// <summary>
    /// Runs <paramref name="transpile"/> on a <see cref="CodeMatcher"/> over
    /// <paramref name="instructions"/> and returns its instructions. If it throws
    /// <see cref="TranspilerMismatchException"/> (e.g. from
    /// <see cref="CodeMatcherExtensions.ExpectForward"/>), logs
    /// <c>Could not {target}, IL does not match expectations: {detail}</c> through
    /// <paramref name="logError"/> and returns the original instructions unchanged.
    /// </summary>
    /// <param name="instructions">The transpiler's incoming instructions.</param>
    /// <param name="target">
    /// What is being patched, including the verb, e.g. <c>patch Dialog_Trade.DoWindowContents</c>
    /// or <c>reverse patch Widgets.TextFieldNumeric</c>.
    /// </param>
    /// <param name="logError">Where to log the mismatch, e.g. the mod's <c>LogError</c>.</param>
    /// <param name="transpile">The transpiler body.</param>
    /// <param name="generator">The transpiler's <see cref="ILGenerator"/>, if it needs one.</param>
    public static IEnumerable<CodeInstruction> PatchOrOriginal(
        IEnumerable<CodeInstruction> instructions,
        string target,
        Action<string> logError,
        Action<CodeMatcher> transpile,
        ILGenerator? generator = null
    )
    {
        ArgumentNullException.ThrowIfNull(logError);
        ArgumentNullException.ThrowIfNull(transpile);

        var original = instructions.ToList();
        var codeMatcher = new CodeMatcher(original, generator);
        try
        {
            transpile(codeMatcher);
        }
        catch (TranspilerMismatchException e)
        {
            logError($"Could not {target}, IL does not match expectations: {e.Detail}");
            return original;
        }

        return codeMatcher.Instructions();
    }

    /// <summary>
    /// Finds the field of the compiler-generated iterator created by the instance iterator method
    /// <paramref name="iteratorMethod"/> that holds <c>this</c>, so a transpiler on the
    /// iterator's <c>MoveNext</c> can load it. The field is the target of the
    /// <c>ldarg.0; stfld</c> in <paramref name="iteratorMethod"/>'s own body.
    /// </summary>
    /// <returns>
    /// The field, or <see langword="null"/> if <paramref name="iteratorMethod"/> is not an
    /// iterator method or does not store <c>this</c> in its iterator.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="iteratorMethod"/> is static.</exception>
    public static FieldInfo? IteratorThisField(MethodBase iteratorMethod)
    {
        ArgumentNullException.ThrowIfNull(iteratorMethod);
        if (iteratorMethod.IsStatic)
        {
            throw new ArgumentException(
                $"{iteratorMethod.FullDescription()} is static and has no this to store.",
                nameof(iteratorMethod)
            );
        }

        var iteratorType = AccessTools.EnumeratorMoveNext(iteratorMethod)?.DeclaringType;
        if (iteratorType is null)
        {
            return null;
        }

        var instructions = PatchProcessor.GetOriginalInstructions(iteratorMethod);
        for (var i = 1; i < instructions.Count; i++)
        {
            if (
                instructions[i - 1].opcode == OpCodes.Ldarg_0
                && instructions[i].opcode == OpCodes.Stfld
                && instructions[i].operand is FieldInfo field
                && field.DeclaringType == iteratorType
            )
            {
                return field;
            }
        }
        return null;
    }

    private static readonly Dictionary<OpCode, OpCode> ShortToLongBranchOpCodes = new()
    {
        [OpCodes.Br_S] = OpCodes.Br,
        [OpCodes.Brfalse_S] = OpCodes.Brfalse,
        [OpCodes.Brtrue_S] = OpCodes.Brtrue,
        [OpCodes.Beq_S] = OpCodes.Beq,
        [OpCodes.Bne_Un_S] = OpCodes.Bne_Un,
        [OpCodes.Bge_S] = OpCodes.Bge,
        [OpCodes.Bgt_S] = OpCodes.Bgt,
        [OpCodes.Ble_S] = OpCodes.Ble,
        [OpCodes.Blt_S] = OpCodes.Blt,
        [OpCodes.Bge_Un_S] = OpCodes.Bge_Un,
        [OpCodes.Bgt_Un_S] = OpCodes.Bgt_Un,
        [OpCodes.Ble_Un_S] = OpCodes.Ble_Un,
        [OpCodes.Blt_Un_S] = OpCodes.Blt_Un,
        [OpCodes.Leave_S] = OpCodes.Leave,
    };

    /// <summary>
    /// Widens every short-form branch instruction (e.g. <c>brfalse.s</c>) to its long-form
    /// equivalent. Harmony's IL emitter does not widen a short branch opcode on its own, so a
    /// pre-existing short branch that still fit its signed-byte range in the original method can
    /// silently overflow into a corrupt offset once a transpiler inserts instructions between it
    /// and its target, pushing the jump past that range. Call this on the incoming instructions
    /// before matching/inserting so branch distances become irrelevant.
    /// </summary>
    public static IEnumerable<CodeInstruction> WidenShortBranches(
        IEnumerable<CodeInstruction> instructions
    )
    {
        ArgumentNullException.ThrowIfNull(instructions);

        return WidenShortBranchesIterator(instructions);
    }

    private static IEnumerable<CodeInstruction> WidenShortBranchesIterator(
        IEnumerable<CodeInstruction> instructions
    )
    {
        foreach (var instruction in instructions)
        {
            if (ShortToLongBranchOpCodes.TryGetValue(instruction.opcode, out var longOpCode))
            {
                instruction.opcode = longOpCode;
            }
            yield return instruction;
        }
    }
}
