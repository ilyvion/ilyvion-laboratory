namespace ilyvion.Laboratory;

#pragma warning disable CS8618 // Set from mod assembly

[StaticConstructorOnStartup]
internal static class Resources
{
    static Resources()
    {
        // This is here to silence the warning about the graphic below being loaded elsewhere.
    }

    // Assigned from StaticConstructorOnStartup in ilyvion.LaboratoryMod project.
    // Can't find a way to disable the stupid warning for this type, though.
    public static Texture2D GraphDot { get; set; }

    public static Texture2D? UpdateMarkAsRead { get; set; }
    public static Texture2D? UpdateHyperlink { get; set; }

    public static Texture2D? AdmonitionNote { get; set; }
    public static Texture2D? AdmonitionTip { get; set; }
    public static Texture2D? AdmonitionImportant { get; set; }
    public static Texture2D? AdmonitionWarning { get; set; }
    public static Texture2D? AdmonitionCaution { get; set; }
}
