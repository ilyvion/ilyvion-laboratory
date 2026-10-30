// TranspilerUtilityTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.Reflection.Emit;
using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class TranspilerUtilityTests
{
    private static void Target() { }

    private static readonly MethodInfo TargetMethod =
        typeof(TranspilerUtilityTests).GetMethod(
            nameof(Target),
            BindingFlags.NonPublic | BindingFlags.Static
        ) ?? throw new MissingMethodException(nameof(TranspilerUtilityTests), nameof(Target));

    [Test]
    public static void InjectsDirectlyBeforeLastMatchWithExtrasBeforeCall()
    {
        var nop1 = new CodeInstruction(OpCodes.Nop);
        var ldc = new CodeInstruction(OpCodes.Ldc_I4_1);
        var extra = new CodeInstruction(OpCodes.Ldarg_0);
        var ret = new CodeInstruction(OpCodes.Ret);

        var result = TranspilerUtility
            .InjectCallBeforeReturn(
                [nop1, ldc, new CodeInstruction(OpCodes.Ldc_I4_1), ret],
                TargetMethod,
                i => i.opcode == OpCodes.Ldc_I4_1,
                [extra]
            )
            .ToList();

        Expect.AreEqual(6, result.Count);
        Expect.AreEqual(OpCodes.Ldc_I4_1, result[1].opcode);
        Expect.AreEqual(OpCodes.Ldarg_0, result[2].opcode);
        Expect.AreEqual(OpCodes.Call, result[3].opcode);
        Expect.AreEqual(TargetMethod, (MethodInfo)result[3].operand);
        Expect.AreEqual(OpCodes.Ldc_I4_1, result[4].opcode);
        Expect.AreEqual(OpCodes.Ret, result[5].opcode);
    }

    [Test]
    public static void InjectsOnlyCallWithoutExtras()
    {
        var result = TranspilerUtility
            .InjectCallBeforeReturn(
                [new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Ret)],
                TargetMethod,
                i => i.opcode == OpCodes.Ldc_I4_1
            )
            .ToList();

        Expect.AreEqual(3, result.Count);
        Expect.AreEqual(OpCodes.Call, result[0].opcode);
        Expect.AreEqual(OpCodes.Ldc_I4_1, result[1].opcode);
        Expect.AreEqual(OpCodes.Ret, result[2].opcode);
    }

    [Test]
    public static void NoMatchThrowsWithInstructions()
    {
        var instructions = new[]
        {
            new CodeInstruction(OpCodes.Nop),
            new CodeInstruction(OpCodes.Ret),
        };
        InjectCallBeforeReturnException? caught = null;
        try
        {
            _ = TranspilerUtility
                .InjectCallBeforeReturn(instructions, TargetMethod, i => i.opcode == OpCodes.Pop)
                .ToList();
        }
        catch (InjectCallBeforeReturnException e)
        {
            caught = e;
        }

        Expect.IsNotNull(caught);
        Expect.AreEqual(2, caught!.Instructions.Count());
    }

    [Test]
    public static void NotEndingInRetThrows()
    {
        InjectCallBeforeReturnException? caught = null;
        try
        {
            _ = TranspilerUtility
                .InjectCallBeforeReturn(
                    [new CodeInstruction(OpCodes.Nop), new CodeInstruction(OpCodes.Throw)],
                    TargetMethod,
                    i => i.opcode == OpCodes.Nop
                )
                .ToList();
        }
        catch (InjectCallBeforeReturnException e)
        {
            caught = e;
        }

        Expect.IsNotNull(caught);
    }

    [Test]
    public static void WidenShortBranchesMapsAllShortOpcodesAndKeepsOperands()
    {
        (OpCode Short, OpCode Long)[] pairs =
        [
            (OpCodes.Br_S, OpCodes.Br),
            (OpCodes.Brfalse_S, OpCodes.Brfalse),
            (OpCodes.Brtrue_S, OpCodes.Brtrue),
            (OpCodes.Beq_S, OpCodes.Beq),
            (OpCodes.Bne_Un_S, OpCodes.Bne_Un),
            (OpCodes.Bge_S, OpCodes.Bge),
            (OpCodes.Bgt_S, OpCodes.Bgt),
            (OpCodes.Ble_S, OpCodes.Ble),
            (OpCodes.Blt_S, OpCodes.Blt),
            (OpCodes.Bge_Un_S, OpCodes.Bge_Un),
            (OpCodes.Bgt_Un_S, OpCodes.Bgt_Un),
            (OpCodes.Ble_Un_S, OpCodes.Ble_Un),
            (OpCodes.Blt_Un_S, OpCodes.Blt_Un),
            (OpCodes.Leave_S, OpCodes.Leave),
        ];
        var label = new Label();
        var input = pairs.Select(p => new CodeInstruction(p.Short, label)).ToList();
        input.Add(new CodeInstruction(OpCodes.Nop));
        input.Add(new CodeInstruction(OpCodes.Br, label));

        var result = TranspilerUtility.WidenShortBranches(input).ToList();

        for (var i = 0; i < pairs.Length; i++)
        {
            Expect.AreEqual(pairs[i].Long, result[i].opcode);
            Expect.AreEqual(label, (Label)result[i].operand);
        }
        Expect.AreEqual(OpCodes.Nop, result[pairs.Length].opcode);
        Expect.AreEqual(OpCodes.Br, result[pairs.Length + 1].opcode);
    }

    [Test]
    public static void PatchOrOriginalReturnsTranspiledInstructionsOnMatch()
    {
        var logged = new List<string>();

        var result = TranspilerUtility
            .PatchOrOriginal(
                [
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Ret),
                ],
                "patch Target",
                logged.Add,
                codeMatcher =>
                {
                    _ = codeMatcher.ExpectForward(i => i.opcode == OpCodes.Ldc_I4_1, "ldc.i4.1");
                    _ = codeMatcher.Insert([new CodeInstruction(OpCodes.Ldarg_0)]);
                }
            )
            .ToList();

        Expect.AreEqual(0, logged.Count);
        Expect.AreEqual(4, result.Count);
        Expect.AreEqual(OpCodes.Ldarg_0, result[1].opcode);
        Expect.AreEqual(OpCodes.Ldc_I4_1, result[2].opcode);
    }

    [Test]
    public static void PatchOrOriginalLogsAndReturnsUnmodifiedOriginalOnMismatch()
    {
        var logged = new List<string>();

        var result = TranspilerUtility
            .PatchOrOriginal(
                [new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Ret)],
                "reverse patch Target",
                logged.Add,
                codeMatcher =>
                {
                    _ = codeMatcher.ExpectForward(i => i.opcode == OpCodes.Ldc_I4_1, "ldc.i4.1");
                    codeMatcher.Opcode = OpCodes.Ldc_I4_2;
                    _ = codeMatcher.ExpectForward(i => i.opcode == OpCodes.Pop, "[pop]");
                }
            )
            .ToList();

        Expect.AreEqual(1, logged.Count);
        Expect.AreEqual(
            "Could not reverse patch Target, IL does not match expectations: [pop] not found.",
            logged[0]
        );
        Expect.AreEqual(2, result.Count);
        Expect.AreEqual(OpCodes.Ldc_I4_1, result[0].opcode);
        Expect.AreEqual(OpCodes.Ret, result[1].opcode);
    }

    [Test]
    public static void PatchOrOriginalLogsDirectlyThrownMismatch()
    {
        var logged = new List<string>();

        _ = TranspilerUtility
            .PatchOrOriginal(
                [new CodeInstruction(OpCodes.Ret)],
                "patch Target",
                logged.Add,
                _ => throw new TranspilerMismatchException("expected ldarg.0; was ret.")
            )
            .ToList();

        Expect.AreEqual(
            "Could not patch Target, IL does not match expectations: expected ldarg.0; was ret.",
            logged[0]
        );
    }

    [Test]
    public static void ExpectBackwardsFindsEarlierInstruction()
    {
        var codeMatcher = new CodeMatcher([
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Nop),
            new CodeInstruction(OpCodes.Ret),
        ]).End();

        _ = codeMatcher.ExpectBackwards(i => i.opcode == OpCodes.Ldarg_0, "ldarg.0");

        Expect.AreEqual(0, codeMatcher.Pos);
    }

    [Test]
    public static void ExpectBackwardsThrowsWhenNotFound()
    {
        var codeMatcher = new CodeMatcher([new CodeInstruction(OpCodes.Ret)]).End();
        TranspilerMismatchException? caught = null;
        try
        {
            _ = codeMatcher.ExpectBackwards(i => i.opcode == OpCodes.Ldarg_0, "ldarg.0");
        }
        catch (TranspilerMismatchException e)
        {
            caught = e;
        }

        Expect.IsNotNull(caught);
        Expect.AreEqual("ldarg.0 not found.", caught!.Detail);
    }

    private sealed class IteratorOwner
    {
        internal IEnumerable<int> Iterate(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return GetHashCode() + i;
            }
        }

        internal int NotAnIterator() => GetHashCode();

        internal static IEnumerable<int> StaticIterate()
        {
            yield return 0;
        }
    }

    [Test]
    public static void IteratorThisFieldFindsFieldHoldingThis()
    {
        var owner = new IteratorOwner();
        var iterator = owner.Iterate(1);

        var field = TranspilerUtility.IteratorThisField(
            AccessTools.Method(typeof(IteratorOwner), nameof(IteratorOwner.Iterate))
        );

        Expect.IsNotNull(field);
        Expect.IsTrue(ReferenceEquals(owner, field!.GetValue(iterator)));
    }

    [Test]
    public static void IteratorThisFieldReturnsNullForNonIterator()
    {
        var field = TranspilerUtility.IteratorThisField(
            AccessTools.Method(typeof(IteratorOwner), nameof(IteratorOwner.NotAnIterator))
        );

        Expect.IsNull(field);
    }

    [Test]
    public static void IteratorThisFieldThrowsForStaticMethod()
    {
        ArgumentException? caught = null;
        try
        {
            _ = TranspilerUtility.IteratorThisField(
                AccessTools.Method(typeof(IteratorOwner), nameof(IteratorOwner.StaticIterate))
            );
        }
        catch (ArgumentException e)
        {
            caught = e;
        }

        Expect.IsNotNull(caught);
        Expect.AreEqual("iteratorMethod", caught!.ParamName);
    }
}
