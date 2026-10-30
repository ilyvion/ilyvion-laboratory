// CodeInstructionsExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.Reflection.Emit;
using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class CodeInstructionsExtensionsTests
{
    private static readonly ConstructorInfo Vector3Constructor =
        typeof(Vector3).GetConstructor([typeof(float), typeof(float), typeof(float)])
        ?? throw new MissingMethodException(nameof(Vector3), ".ctor");

    private static readonly ConstructorInfo Vector2Constructor =
        typeof(Vector2).GetConstructor([typeof(float), typeof(float)])
        ?? throw new MissingMethodException(nameof(Vector2), ".ctor");

    private static readonly ConstructorInfo ListConstructor =
        typeof(List<int>).GetConstructor(Type.EmptyTypes)
        ?? throw new MissingMethodException("List<int>", ".ctor");

    private static readonly MethodInfo ToStringMethod =
        typeof(object).GetMethod(nameof(ToString))
        ?? throw new MissingMethodException("object", nameof(ToString));

    [Test]
    public static void CallsConstructorMatchesCallAndNewobjOfThatConstructor()
    {
        Expect.IsTrue(
            new CodeInstruction(OpCodes.Call, Vector3Constructor).CallsConstructor(
                Vector3Constructor
            )
        );
        Expect.IsTrue(
            new CodeInstruction(OpCodes.Newobj, ListConstructor).CallsConstructor(ListConstructor)
        );
    }

    [Test]
    public static void CallsConstructorRejectsOtherConstructorsOpcodesAndOperands()
    {
        Expect.IsFalse(
            new CodeInstruction(OpCodes.Call, Vector2Constructor).CallsConstructor(
                Vector3Constructor
            )
        );
        Expect.IsFalse(
            new CodeInstruction(OpCodes.Ldtoken, Vector3Constructor).CallsConstructor(
                Vector3Constructor
            )
        );
        Expect.IsFalse(
            new CodeInstruction(OpCodes.Call, ToStringMethod).CallsConstructor(Vector3Constructor)
        );
    }

    [Test]
    public static void CallsConstructorOfMatchesAnyConstructorOfTheType()
    {
        Expect.IsTrue(
            new CodeInstruction(OpCodes.Call, Vector3Constructor).CallsConstructorOf(
                typeof(Vector3)
            )
        );
        Expect.IsTrue(
            new CodeInstruction(OpCodes.Newobj, ListConstructor).CallsConstructorOf(
                typeof(List<int>)
            )
        );
    }

    [Test]
    public static void CallsConstructorOfRejectsOtherTypesOpcodesAndOperands()
    {
        Expect.IsFalse(
            new CodeInstruction(OpCodes.Call, Vector2Constructor).CallsConstructorOf(
                typeof(Vector3)
            )
        );
        Expect.IsFalse(
            new CodeInstruction(OpCodes.Callvirt, Vector3Constructor).CallsConstructorOf(
                typeof(Vector3)
            )
        );
        Expect.IsFalse(
            new CodeInstruction(OpCodes.Call, ToStringMethod).CallsConstructorOf(typeof(object))
        );
    }

    [Test]
    public static void LocalVariableIndexReadsShortFormLoadsAndStores()
    {
        Expect.AreEqual(0, new CodeInstruction(OpCodes.Ldloc_0).LocalVariableIndex());
        Expect.AreEqual(1, new CodeInstruction(OpCodes.Stloc_1).LocalVariableIndex());
        Expect.AreEqual(2, new CodeInstruction(OpCodes.Ldloc_2).LocalVariableIndex());
        Expect.AreEqual(3, new CodeInstruction(OpCodes.Stloc_3).LocalVariableIndex());
    }

    [Test]
    public static void LocalVariableIndexReadsOperandOfLongFormLoadsAddressesAndStores()
    {
        Expect.AreEqual(4, new CodeInstruction(OpCodes.Ldloc_S, (byte)4).LocalVariableIndex());
        Expect.AreEqual(5, new CodeInstruction(OpCodes.Ldloca_S, 5).LocalVariableIndex());
        Expect.AreEqual(6, new CodeInstruction(OpCodes.Stloc_S, (byte)6).LocalVariableIndex());
        Expect.AreEqual(300, new CodeInstruction(OpCodes.Ldloc, (short)300).LocalVariableIndex());
        Expect.AreEqual(301, new CodeInstruction(OpCodes.Ldloca, 301).LocalVariableIndex());
        Expect.AreEqual(302, new CodeInstruction(OpCodes.Stloc, 302).LocalVariableIndex());
    }

    [Test]
    public static void LocalVariableIndexReadsLocalBuilderOperand()
    {
        var method = new DynamicMethod("Locals", typeof(void), Type.EmptyTypes);
        var generator = method.GetILGenerator();
        _ = generator.DeclareLocal(typeof(int));
        var local = generator.DeclareLocal(typeof(string));

        Expect.AreEqual(1, new CodeInstruction(OpCodes.Stloc_S, local).LocalVariableIndex());
    }

    [Test]
    public static void LocalVariableIndexRejectsInstructionsThatDoNotAccessALocal()
    {
        _ = Expect.Throws<ArgumentException>(() =>
            new CodeInstruction(OpCodes.Ldarg_0).LocalVariableIndex()
        );
        _ = Expect.Throws<ArgumentException>(() =>
            new CodeInstruction(OpCodes.Ldloc_S, "not a local").LocalVariableIndex()
        );
    }

    [Test]
    public static void LoadsLocalMatchesValueLoadsOfThatLocal()
    {
        Expect.IsTrue(new CodeInstruction(OpCodes.Ldloc_2).LoadsLocal(2));
        Expect.IsTrue(new CodeInstruction(OpCodes.Ldloc_S, (byte)7).LoadsLocal(7));
        Expect.IsTrue(new CodeInstruction(OpCodes.Ldloc, 300).LoadsLocal(300));
    }

    [Test]
    public static void LoadsLocalRejectsOtherLocalsAddressLoadsAndStores()
    {
        Expect.IsFalse(new CodeInstruction(OpCodes.Ldloc_2).LoadsLocal(3));
        Expect.IsFalse(new CodeInstruction(OpCodes.Ldloca_S, (byte)7).LoadsLocal(7));
        Expect.IsFalse(new CodeInstruction(OpCodes.Stloc_S, (byte)7).LoadsLocal(7));
        Expect.IsFalse(new CodeInstruction(OpCodes.Ldarg_0).LoadsLocal(0));
    }

    [Test]
    public static void LoadSameLocalLoadsTheAccessedLocal()
    {
        var shortForm = new CodeInstruction(OpCodes.Stloc_3).LoadSameLocal();
        Expect.AreEqual(OpCodes.Ldloc_3, shortForm.opcode);
        Expect.IsNull(shortForm.operand);

        var byteForm = new CodeInstruction(OpCodes.Ldloca_S, (byte)4).LoadSameLocal();
        Expect.AreEqual(OpCodes.Ldloc_S, byteForm.opcode);
        Expect.AreEqual(4, byteForm.LocalVariableIndex());

        var longForm = new CodeInstruction(OpCodes.Stloc, 300).LoadSameLocal();
        Expect.AreEqual(OpCodes.Ldloc, longForm.opcode);
        Expect.AreEqual(300, longForm.LocalVariableIndex());
    }

    [Test]
    public static void StoreSameLocalStoresIntoTheAccessedLocal()
    {
        var shortForm = new CodeInstruction(OpCodes.Ldloc_0).StoreSameLocal();
        Expect.AreEqual(OpCodes.Stloc_0, shortForm.opcode);
        Expect.IsNull(shortForm.operand);

        var byteForm = new CodeInstruction(OpCodes.Ldloc_S, (byte)8).StoreSameLocal();
        Expect.AreEqual(OpCodes.Stloc_S, byteForm.opcode);
        Expect.AreEqual(8, byteForm.LocalVariableIndex());

        var longForm = new CodeInstruction(OpCodes.Ldloc, 300).StoreSameLocal();
        Expect.AreEqual(OpCodes.Stloc, longForm.opcode);
        Expect.AreEqual(300, longForm.LocalVariableIndex());
    }
}
