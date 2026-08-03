using ilyvion.Laboratory;

namespace ilyvion.LaboratoryMod;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Class is instantiated via reflection"
)]
internal sealed class IlyvionsLaboratoryMod : IlyvionMod
{
#pragma warning disable CS8618 // Set to non-null before it matters.
    internal static IlyvionsLaboratoryMod _mod;
#pragma warning restore CS8618

    protected override bool HasSettings => true;

    internal IlyvionsLaboratorySettings Settings { get; }

    public IlyvionsLaboratoryMod(ModContentPack content)
        : base(content)
    {
        _mod = this;

        // Harmony.DEBUG = true;
#if !v1_3 && !v1_4
        new Harmony(Content.Name).PatchAllUncategorized(Assembly.GetExecutingAssembly());
#else
        new Harmony(Content.Name).PatchAll(Assembly.GetExecutingAssembly());
#endif
        // Harmony.DEBUG = false;

        // Inject reverse patch method pointers where they are needed
        CustomStreamReaderScribeLoader.initLoadingWithCustomStreamReader =
            Verse_ScribeLoader_InitLoading_Reverse.InitLoadingWithCustomStreamReader;
        CustomStreamScribeSaver.initSavingWithCustomStream =
            Verse_ScribeSaver_InitSaving_Reverse.InitSavingWithCustomStream;

        UpdateNotifications.SaveSettings = WriteSettings;

        Settings = GetSettings<IlyvionsLaboratorySettings>();
        if (!Settings.HasInitializedUpdateCutoff)
        {
            UpdateNotifications.CutoffDate = DateTime.Today;
            Settings.HasInitializedUpdateCutoff = true;
            WriteSettings();
        }
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        IlyvionsLaboratorySettings.DoWindowContents(inRect);
    }
}

#if !v1_3 && !v1_4
[StaticConstructorOnStartup]
internal static class LatePatching
{
    static LatePatching()
    {
        // Harmony.DEBUG = true;
        new Harmony(IlyvionsLaboratoryMod._mod.Content.Name).PatchCategory(
            Assembly.GetExecutingAssembly(),
            "Late"
        );
        // Harmony.DEBUG = false;
    }
}
#endif

[StaticConstructorOnStartup]
internal static class ResourceLoading
{
    static ResourceLoading()
    {
        Laboratory.Resources.GraphDot = ContentFinder<Texture2D>.Get(
            "UI/Icons/ilyvion.Laboratory.GraphDot"
        );
        Laboratory.Resources.UpdateMarkAsRead = ContentFinder<Texture2D>.Get(
            "UI/Icons/Updates/MarkAsRead"
        );
        Laboratory.Resources.UpdateHyperlink = ContentFinder<Texture2D>.Get(
            "UI/Icons/Updates/Hyperlink"
        );
        Laboratory.Resources.AdmonitionNote = ContentFinder<Texture2D>.Get(
            "UI/Icons/Updates/Information"
        );
        Laboratory.Resources.AdmonitionTip = ContentFinder<Texture2D>.Get("UI/Icons/Updates/Tip");
        Laboratory.Resources.AdmonitionImportant = ContentFinder<Texture2D>.Get(
            "UI/Icons/Updates/Important"
        );
        Laboratory.Resources.AdmonitionWarning = ContentFinder<Texture2D>.Get(
            "UI/Icons/Updates/Warning"
        );
        Laboratory.Resources.AdmonitionCaution = ContentFinder<Texture2D>.Get(
            "UI/Icons/Updates/Caution"
        );
    }
}
