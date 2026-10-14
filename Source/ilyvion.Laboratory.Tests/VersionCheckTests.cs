// VersionCheckTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class VersionCheckTests
{
    // Regression guard for BUG-19: IsAtLeastVersion used > instead of >=, so checking a
    // dependency against the exact version currently loaded reported it as too old.
    [Test]
    public static void IsAtLeastVersionIsTrueForExactVersionMatch()
    {
        var result = VersionCheck.IsAtLeastVersion(VersionCheck.OurVersion);

        Expect.IsTrue(result);
    }

    [Test]
    public static void IsAtLeastVersionIsTrueForOlderRequiredVersion()
    {
        var older = new Version(0, 0, 0, 0);

        var result = VersionCheck.IsAtLeastVersion(older);

        Expect.IsTrue(result);
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

        Expect.IsFalse(result);
    }

    [Test]
    public static void VersionCheckDefModIdDefaultsToOurOwnModId()
    {
        var def = new VersionCheckDef();

        Expect.AreEqual(VersionCheck.OurModId, def.modId);
    }

    [Test]
    public static void VersionCheckDefRequiredVersionUsesTwoPartCtorWhenBuildAndRevisionUnset()
    {
        var def = new VersionCheckDef { majorVersion = 1, minorVersion = 2 };

        var result = def.RequiredVersion;

        Expect.AreEqual(new Version(1, 2), result);
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

        Expect.AreEqual(new Version(1, 2, 3), result);
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

        Expect.AreEqual(new Version(1, 2, 3, 4), result);
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

        Expect.AreEqual(new Version(1, 2), result);
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

        Expect.IsTrue(
            errors
                .ToList()
                .Contains(
                    "revisionVersion is set without buildVersion; revisionVersion is ignored unless buildVersion is also set"
                )
        );
    }
}
