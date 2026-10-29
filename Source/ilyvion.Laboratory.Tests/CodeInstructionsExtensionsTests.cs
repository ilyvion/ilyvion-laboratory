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
}
