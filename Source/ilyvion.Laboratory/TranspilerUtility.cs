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
