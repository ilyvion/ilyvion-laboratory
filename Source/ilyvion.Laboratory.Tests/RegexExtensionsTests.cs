// RegexExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.Text.RegularExpressions;
using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class RegexExtensionsTests
{
    [Test]
    public static void InstanceCountReturnsTheNumberOfMatches()
    {
        var regex = new Regex("a+");

        Expect.AreEqual(3, regex.Count("a baa caaa"));
        Expect.AreEqual(0, regex.Count("xyz"));
    }

    [Test]
    public static void StaticCountReturnsTheNumberOfMatches()
    {
        Expect.AreEqual(2, Regex.Count("pawn, Pawn, PAWN, pawn", "pawn"));
        Expect.AreEqual(0, Regex.Count("", "pawn"));
    }

    [Test]
    public static void StaticCountAppliesTheOptions()
    {
        Expect.AreEqual(4, Regex.Count("pawn, Pawn, PAWN, pawn", "pawn", RegexOptions.IgnoreCase));
        Expect.AreEqual(
            4,
            Regex.Count(
                "pawn, Pawn, PAWN, pawn",
                "pawn",
                RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(1)
            )
        );
    }
}
