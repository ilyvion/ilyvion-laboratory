// BoxedTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class BoxedTests
{
    [Test]
    public static void ValueGetReturnsInitialValue()
    {
        var boxed = new Boxed<int>(5);

        Assert.That(boxed.Value).Is.EqualTo(5);
    }

    [Test]
    public static void ValueSetUpdatesValue()
    {
        var boxed = new Boxed<int>(5) { Value = 10 };

        Assert.That(boxed.Value).Is.EqualTo(10);
    }

    [Test]
    public static void RefValueMutatesThroughReference()
    {
        var boxed = new Boxed<int>(5);
        Assert.That(boxed.Value).Is.EqualTo(5);

        boxed.RefValue = 42;

        Assert.That(boxed.Value).Is.EqualTo(42);
    }

    [Test]
    public static void ImplicitConversionToValueReturnsCurrentValue()
    {
        var boxed = new Boxed<int>(7);

        int converted = boxed;

        Assert.That(converted).Is.EqualTo(7);
    }

    [Test]
    public static void ExplicitConversionFromValueCreatesBoxedWithThatValue()
    {
        var boxed = (Boxed<int>)7;

        Assert.That(boxed.Value).Is.EqualTo(7);
    }

    [Test]
    public static void ToAnyBoxedCopiesCurrentValue()
    {
        var boxed = new Boxed<int>(3);

        var anyBoxed = boxed.ToAnyBoxed();

        Assert.That(anyBoxed.Value).Is.EqualTo(3);
    }

    [Test]
    public static void AnyBoxedValueGetReturnsInitialValue()
    {
        var anyBoxed = new AnyBoxed<string>("hello");

        Assert.That(anyBoxed.Value).Is.EqualTo("hello");
    }

    [Test]
    public static void AnyBoxedValueSetUpdatesValue()
    {
        var anyBoxed = new AnyBoxed<string>("hello") { Value = "world" };

        Assert.That(anyBoxed.Value).Is.EqualTo("world");
    }

    [Test]
    public static void AnyBoxedRefValueMutatesThroughReference()
    {
        var anyBoxed = new AnyBoxed<string>("hello");
        Assert.That(anyBoxed.Value).Is.EqualTo("hello");

        anyBoxed.RefValue = "world";

        Assert.That(anyBoxed.Value).Is.EqualTo("world");
    }

    [Test]
    public static void AnyBoxedImplicitConversionToValueReturnsCurrentValue()
    {
        var anyBoxed = new AnyBoxed<string>("hello");

        string converted = anyBoxed;

        Assert.That(converted).Is.EqualTo("hello");
    }

    [Test]
    public static void AnyBoxedExplicitConversionFromValueCreatesBoxedWithThatValue()
    {
        var anyBoxed = (AnyBoxed<string>)"hello";

        Assert.That(anyBoxed.Value).Is.EqualTo("hello");
    }
}
