namespace ilyvion.Laboratory;

public class CustomFontManager
{
    internal static bool featureEnabled;

    public static void EnableFeature() => featureEnabled = true;

    public static CustomFontManager Instance
    {
        get
        {
            if (field != null)
            {
                return field;
            }
            field = new CustomFontManager();
            return field;
        }
    }

    private readonly Dictionary<string, (int size, string[] fonts)> customFontParams = [];
    private readonly Dictionary<string, Font> customFonts = [];
    private readonly Dictionary<string, float> customFontLineHeights = [];
    private readonly Dictionary<string, float> customFontSpaceBetweenLines = [];
    private readonly Dictionary<string, GUIStyle> customFontStyles = [];
    private readonly Dictionary<string, GUIStyle> customTextFieldStyles = [];
    private readonly Dictionary<string, GUIStyle> customTextAreaStyles = [];
    private readonly Dictionary<string, GUIStyle> customTextAreaReadOnlyStyles = [];

    public string? CurrentFontKey { get; private set; }

    internal Font? CurrentFont
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            if (CurrentFontKey == null)
            {
                return null;
            }

            if (!customFonts.TryGetValue(CurrentFontKey, out field))
            {
                if (customFontParams.TryGetValue(CurrentFontKey, out var fontParams))
                {
                    field = Font.CreateDynamicFontFromOSFont(fontParams.fonts, fontParams.size);
                    customFonts.Add(CurrentFontKey, field);
                }
            }

            return field;
        }
        private set;
    }

    internal float? CurrentLineHeight
    {
        get
        {
            if (field.HasValue)
            {
                return field;
            }

            if (CurrentFontKey == null)
            {
                return null;
            }

            if (customFontLineHeights.TryGetValue(CurrentFontKey, out var currentLineHeight))
            {
                field = currentLineHeight;
            }
            else
            {
                currentLineHeight = Text.CalcHeight("W", 999f);
                field = currentLineHeight;
                customFontLineHeights.Add(CurrentFontKey, currentLineHeight);
            }

            return field;
        }
        private set;
    }

    internal float? CurrentSpaceBetweenLines
    {
        get
        {
            if (field.HasValue)
            {
                return field;
            }

            if (CurrentFontKey == null)
            {
                return null;
            }

            if (
                customFontSpaceBetweenLines.TryGetValue(
                    CurrentFontKey,
                    out var currentSpaceBetweenLines
                )
            )
            {
                field = currentSpaceBetweenLines;
            }
            else
            {
                currentSpaceBetweenLines =
                    Text.CalcHeight("W\nW", 999f) - (Text.CalcHeight("W", 999f) * 2f);
                field = currentSpaceBetweenLines;
                customFontSpaceBetweenLines.Add(CurrentFontKey, currentSpaceBetweenLines);
            }

            return field;
        }
        private set;
    }

    internal GUIStyle? CurrentFontStyle
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            if (CurrentFontKey == null)
            {
                return null;
            }

            if (!customFontStyles.TryGetValue(CurrentFontKey, out field))
            {
                var currentFont = CurrentFont;
                if (currentFont != null)
                {
                    field = new(GUI.skin.label) { font = currentFont };
                    customFontStyles.Add(CurrentFontKey, field);
                }
            }

            return field;
        }
        private set;
    }

    internal GUIStyle? CurrentTextFieldStyle
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            if (CurrentFontKey == null)
            {
                return null;
            }

            if (!customTextFieldStyles.TryGetValue(CurrentFontKey, out field))
            {
                var currentFont = CurrentFont;
                if (currentFont != null)
                {
                    field = new(GUI.skin.textField)
                    {
                        font = currentFont,
                        alignment = TextAnchor.MiddleLeft,
                    };
                    customTextFieldStyles.Add(CurrentFontKey, field);
                }
            }

            return field;
        }
        private set;
    }

    internal GUIStyle? CurrentTextAreaStyle
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            if (CurrentFontKey == null)
            {
                return null;
            }

            if (!customTextAreaStyles.TryGetValue(CurrentFontKey, out field))
            {
                var currentFont = CurrentFont;
                if (currentFont != null)
                {
                    field = new(GUI.skin.textField)
                    {
                        font = currentFont,
                        alignment = TextAnchor.UpperLeft,
                        wordWrap = true,
                    };
                    customTextAreaStyles.Add(CurrentFontKey, field);
                }
            }

            return field;
        }
        private set;
    }

    internal GUIStyle? CurrentTextAreaReadOnlyStyle
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            if (CurrentFontKey == null)
            {
                return null;
            }

            if (!customTextAreaReadOnlyStyles.TryGetValue(CurrentFontKey, out field))
            {
                var currentFont = CurrentFont;
                if (currentFont != null)
                {
                    field = new(GUI.skin.textField)
                    {
                        font = currentFont,
                        alignment = TextAnchor.UpperLeft,
                        wordWrap = true,
                    };
                    field.normal.background = null;
                    field.active.background = null;
                    field.onHover.background = null;
                    field.hover.background = null;
                    field.onFocused.background = null;
                    field.focused.background = null;
                    customTextAreaReadOnlyStyles.Add(CurrentFontKey, field);
                }
            }

            return field;
        }
        private set;
    }

    private static void LogFeatureNotEnabled() =>
        Logger.LogError(
            "CustomFontManager is not active. Make sure you call CustomFontManager.EnableFeature() in your mod's constructor to enable it."
        );

    public void AddFont(string key, int size, params string[] fonts)
    {
        if (!featureEnabled)
        {
            LogFeatureNotEnabled();
            return;
        }
        customFontParams.Add(key, (size, fonts));
    }

    public void UseFont(string key)
    {
        if (!featureEnabled)
        {
            LogFeatureNotEnabled();
            return;
        }

        ClearFont();
        CurrentFontKey = key;
    }

    public void ClearFont()
    {
        if (!featureEnabled)
        {
            LogFeatureNotEnabled();
            return;
        }

        CurrentFontKey = null;
        CurrentFont = null;
        CurrentFontStyle = null;
        CurrentTextFieldStyle = null;
        CurrentTextAreaStyle = null;
        CurrentTextAreaReadOnlyStyle = null;
        CurrentLineHeight = null;
        CurrentSpaceBetweenLines = null;
    }
}
