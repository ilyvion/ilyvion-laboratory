// EnumExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class EnumExtensionsTests
{
    private enum Sample
    {
        First = 1,
        Second = 2,
        Fourth = 4,
    }

    [Test]
    public static void GetValuesReturnsEveryConstantInValueOrder()
    {
        var values = Enum.GetValues<Sample>();

        Expect.AreEqual(3, values.Length);
        Expect.AreEqual(Sample.First, values[0]);
        Expect.AreEqual(Sample.Second, values[1]);
        Expect.AreEqual(Sample.Fourth, values[2]);
    }

    [Test]
    public static void GetNamesReturnsEveryConstantNameInValueOrder()
    {
        var names = Enum.GetNames<Sample>();

        Expect.AreEqual(3, names.Length);
        Expect.AreEqual("First", names[0]);
        Expect.AreEqual("Second", names[1]);
        Expect.AreEqual("Fourth", names[2]);
    }

    [Test]
    public static void GetNameReturnsTheConstantNameOrNull()
    {
        Expect.AreEqual("Second", Enum.GetName(Sample.Second));
        Expect.IsNull(Enum.GetName((Sample)3));
    }

    [Test]
    public static void IsDefinedIsTrueOnlyForConstantValues()
    {
        Expect.IsTrue(Enum.IsDefined(Sample.Fourth));
        Expect.IsFalse(Enum.IsDefined((Sample)3));
    }

    [Test]
    public static void ParseConvertsNamesAndNumericValues()
    {
        Expect.AreEqual(Sample.Second, Enum.Parse<Sample>("Second"));
        Expect.AreEqual(Sample.Fourth, Enum.Parse<Sample>("4"));
        _ = Expect.Throws<ArgumentException>(() => Enum.Parse<Sample>("second"));
    }

    [Test]
    public static void ParseWithIgnoreCaseConvertsNamesRegardlessOfCase()
    {
        Expect.AreEqual(Sample.Second, Enum.Parse<Sample>("second", ignoreCase: true));
        _ = Expect.Throws<ArgumentException>(() => Enum.Parse<Sample>("second", ignoreCase: false));
    }
}
