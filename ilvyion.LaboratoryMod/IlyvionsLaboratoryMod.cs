
using System.Reflection;
using ilvyion.Laboratory;

namespace ilvyion.LaboratoryMod;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Class is instantiated via reflection")]
internal sealed class IlyvionsLaboratoryMod : IlyvionMod
{
#pragma warning disable CS8618 // Set to non-null before it matters.
    internal static IlyvionsLaboratoryMod _mod;
#pragma warning restore CS8618

    public IlyvionsLaboratoryMod(ModContentPack content) : base(content)
    {
        _mod = this;

        // Harmony.DEBUG = true;
        new Harmony(content.Name).PatchAll(Assembly.GetExecutingAssembly());
        // Harmony.DEBUG = false;

        // Inject reverse patch method pointers where they are needed
        CustomStreamReaderScribeLoader.initLoadingWithCustomStreamReader = Verse_ScribeLoader_InitLoading_Reverse.InitLoadingWithCustomStreamReader;
        CustomStreamScribeSaver.initSavingWithCustomStream = Verse_ScribeSaver_InitSaving_Reverse.InitSavingWithCustomStream;
    }
}
