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

    [Test]
    public static void VersionCheckDefModIdDefaultsToOurOwnModId()
    {
        var def = new VersionCheckDef();

        Assert.That(def.modId).Is.EqualTo(VersionCheck.OurModId);
    }

    [Test]
    public static void VersionCheckDefRequiredVersionUsesTwoPartCtorWhenBuildAndRevisionUnset()
    {
        var def = new VersionCheckDef { majorVersion = 1, minorVersion = 2 };

        var result = def.RequiredVersion;

        Assert.That(result).Is.EqualTo(new Version(1, 2));
    }

    [Test]
    public static void VersionCheckDefRequiredVersionUsesThreePartCtorWhenOnlyBuildSet()
    {
        var def = new VersionCheckDef
        {
            majorVersion = 1,
            minorVersion = 2,
            buildVersion = 3,
        };

        var result = def.RequiredVersion;

        Assert.That(result).Is.EqualTo(new Version(1, 2, 3));
    }

    [Test]
    public static void VersionCheckDefRequiredVersionUsesFourPartCtorWhenBuildAndRevisionSet()
    {
        var def = new VersionCheckDef
        {
            majorVersion = 1,
            minorVersion = 2,
            buildVersion = 3,
            revisionVersion = 4,
        };

        var result = def.RequiredVersion;

        Assert.That(result).Is.EqualTo(new Version(1, 2, 3, 4));
    }

    [Test]
    public static void VersionCheckDefRequiredVersionIgnoresRevisionWithoutBuild()
    {
        var def = new VersionCheckDef
        {
            majorVersion = 1,
            minorVersion = 2,
            revisionVersion = 4,
        };

        var result = def.RequiredVersion;

        Assert.That(result).Is.EqualTo(new Version(1, 2));
    }

    [Test]
    public static void VersionCheckDefConfigErrorsFlagsRevisionVersionSetWithoutBuildVersion()
    {
        var def = new VersionCheckDef
        {
            majorVersion = 1,
            minorVersion = 2,
            revisionVersion = 4,
            modName = "Some Mod",
        };

        var errors = def.ConfigErrors();

        Assert
            .ThatCollection(errors.ToList())
            .Does.Contain(
                "revisionVersion is set without buildVersion; revisionVersion is ignored unless buildVersion is also set"
            );
    }
}
