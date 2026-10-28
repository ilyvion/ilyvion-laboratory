// IlyvionModTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class IlyvionModTests
{
    private const string CustomHarmonyId = "ilyvion.laboratory.tests.ilyvionmod";

    private sealed class TestMod(ModContentPack content) : IlyvionMod(content)
    {
        public Harmony CallPatchAll(string? harmonyId = null) => PatchAll(harmonyId);

        public void CallLoadSettingsWhenFinished() => LoadSettingsWhenFinished<TestSettings>();
    }

    private sealed class TestSettings : ModSettings
    {
        public static int CreatedCount { get; set; }

        public TestSettings()
        {
            CreatedCount++;
        }
    }

    private static TestMod CreateMod() =>
        new(
            LoadedModManager.RunningModsListForReading.First(m =>
                m.PackageId == "ilyvion.laboratory"
            )
        );

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int PatchTarget() => 1;

    [HarmonyPatch(typeof(IlyvionModTests), nameof(PatchTarget))]
    private static class PatchTargetPatch
    {
        internal static void Postfix(ref int __result) => __result = 2;
    }

    [Test]
    public static void PatchAllAppliesThePatchesInTheModClassAssembly()
    {
        var harmony = CreateMod().CallPatchAll(CustomHarmonyId);
        try
        {
            Expect.AreEqual(2, PatchTarget());
        }
        finally
        {
            harmony.UnpatchAll(CustomHarmonyId);
        }
    }

    [Test]
    public static void PatchAllUsesTheGivenHarmonyId()
    {
        var harmony = CreateMod().CallPatchAll(CustomHarmonyId);
        try
        {
            Expect.AreEqual(CustomHarmonyId, harmony.Id);
        }
        finally
        {
            harmony.UnpatchAll(CustomHarmonyId);
        }
    }

    [Test]
    public static void PatchAllDefaultsToThePackageIdAsHarmonyId()
    {
        var mod = CreateMod();
        var harmony = mod.CallPatchAll();
        try
        {
            Expect.AreEqual(mod.Content.PackageId, harmony.Id);
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    [Test]
    public static void LoadSettingsWhenFinishedLoadsTheSettingsOutsideALongEvent()
    {
        TestSettings.CreatedCount = 0;
        var mod = CreateMod();

        mod.CallLoadSettingsWhenFinished();

        Expect.AreEqual(1, TestSettings.CreatedCount);
        _ = mod.GetSettings<TestSettings>();
        Expect.AreEqual(1, TestSettings.CreatedCount);
    }
}
