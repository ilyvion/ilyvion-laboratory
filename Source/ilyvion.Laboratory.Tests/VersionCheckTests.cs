// VersionCheckTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class VersionCheckTests
{
    // Regression guard for BUG-19: IsAtLeastVersion used > instead of >=, so checking a
    // dependency against the exact version currently loaded reported it as too old.
    [Test]
    public static void IsAtLeastVersionIsTrueForExactVersionMatch()
    {
        var result = VersionCheck.IsAtLeastVersion(VersionCheck.OurVersion);

        Assert.That(result).Is.True();
    }

    [Test]
    public static void IsAtLeastVersionIsTrueForOlderRequiredVersion()
    {
        var older = new Version(0, 0, 0, 0);

        var result = VersionCheck.IsAtLeastVersion(older);

        Assert.That(result).Is.True();
    }

    [Test]
    public static void IsAtLeastVersionIsFalseForNewerRequiredVersion()
    {
        var newer = new Version(
            VersionCheck.OurVersion.Major,
            VersionCheck.OurVersion.Minor,
            VersionCheck.OurVersion.Build,
            VersionCheck.OurVersion.Revision + 1
        );

        var result = VersionCheck.IsAtLeastVersion(newer);

        Assert.That(result).Is.False();
    }
}
