// BoxedTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class BoxedTests
{
    [Test]
    public static void ValueGetReturnsInitialValue()
    {
        var boxed = new Boxed<int>(5);

        Expect.AreEqual(5, boxed.Value);
    }

    [Test]
    public static void ValueSetUpdatesValue()
    {
        var boxed = new Boxed<int>(5) { Value = 10 };

        Expect.AreEqual(10, boxed.Value);
    }

    [Test]
    public static void RefValueMutatesThroughReference()
    {
        var boxed = new Boxed<int>(5);
        Expect.AreEqual(5, boxed.Value);

        boxed.RefValue = 42;

        Expect.AreEqual(42, boxed.Value);
    }

    [Test]
    public static void ImplicitConversionToValueReturnsCurrentValue()
    {
        var boxed = new Boxed<int>(7);

        int converted = boxed;

        Expect.AreEqual(7, converted);
    }

    [Test]
    public static void ExplicitConversionFromValueCreatesBoxedWithThatValue()
    {
        var boxed = (Boxed<int>)7;

        Expect.AreEqual(7, boxed.Value);
    }

    [Test]
    public static void ToAnyBoxedCopiesCurrentValue()
    {
        var boxed = new Boxed<int>(3);

        var anyBoxed = boxed.ToAnyBoxed();

        Expect.AreEqual(3, anyBoxed.Value);
    }

    [Test]
    public static void AnyBoxedValueGetReturnsInitialValue()
    {
        var anyBoxed = new AnyBoxed<string>("hello");

        Expect.AreEqual("hello", anyBoxed.Value);
    }

    [Test]
    public static void AnyBoxedValueSetUpdatesValue()
    {
        var anyBoxed = new AnyBoxed<string>("hello") { Value = "world" };

        Expect.AreEqual("world", anyBoxed.Value);
    }

    [Test]
    public static void AnyBoxedRefValueMutatesThroughReference()
    {
        var anyBoxed = new AnyBoxed<string>("hello");
        Expect.AreEqual("hello", anyBoxed.Value);

        anyBoxed.RefValue = "world";

        Expect.AreEqual("world", anyBoxed.Value);
    }

    [Test]
    public static void AnyBoxedImplicitConversionToValueReturnsCurrentValue()
    {
        var anyBoxed = new AnyBoxed<string>("hello");

        string converted = anyBoxed;

        Expect.AreEqual("hello", converted);
    }

    [Test]
    public static void AnyBoxedExplicitConversionFromValueCreatesBoxedWithThatValue()
    {
        var anyBoxed = (AnyBoxed<string>)"hello";

        Expect.AreEqual("hello", anyBoxed.Value);
    }
}
