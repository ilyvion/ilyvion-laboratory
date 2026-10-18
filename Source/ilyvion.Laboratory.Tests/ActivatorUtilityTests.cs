// ActivatorUtilityTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class ActivatorUtilityTests
{
    private class Sample(string value)
    {
        public string Value { get; } = value;

        public Sample()
            : this("default") { }
    }

    private sealed class DerivedSample : Sample;

    [Test]
    public static void CreateInstanceUsesTheParameterlessConstructorWithoutArguments()
    {
        var instance = ActivatorUtility.CreateInstance<Sample>(typeof(Sample));

        Expect.AreEqual("default", instance.Value);
    }

    [Test]
    public static void CreateInstancePassesArgumentsToTheConstructor()
    {
        var instance = ActivatorUtility.CreateInstance<Sample>(typeof(Sample), "given");

        Expect.AreEqual("given", instance.Value);
    }

    [Test]
    public static void CreateInstanceCreatesTheGivenTypeCastToTheRequestedType()
    {
        var instance = ActivatorUtility.CreateInstance<Sample>(typeof(DerivedSample));

        Expect.IsTrue(instance.GetType() == typeof(DerivedSample));
    }

    [Test]
    public static void CreateInstanceThrowsOnNullType() =>
        _ = Expect.Throws<InvalidOperationException>(() =>
            ActivatorUtility.CreateInstance<Sample>(null)
        );

    [Test]
    public static void CreateInstanceThrowsOnNullableValueType() =>
        _ = Expect.Throws<InvalidOperationException>(() =>
            ActivatorUtility.CreateInstance<int?>(typeof(int?))
        );
}
