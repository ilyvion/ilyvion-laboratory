namespace ilvyion.Laboratory;

public class CustomFontManager
{
    internal static bool featureEnabled;
    public static void EnableFeature()
    {
        featureEnabled = true;
    }

    private static CustomFontManager? _instance;
    public static CustomFontManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            _instance = new CustomFontManager();
            return _instance;
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

    private string? currentFontKey;
    public string? CurrentFontKey => currentFontKey;

    private Font? currentFontCached;
    internal Font? CurrentFont
    {
        get
        {
            if (currentFontCached != null)
            {
                return currentFontCached;
            }

            if (currentFontKey == null)
            {
                return null;
            }

            if (!customFonts.TryGetValue(currentFontKey, out currentFontCached))
            {
                if (customFontParams.TryGetValue(currentFontKey, out var fontParams))
                {
                    currentFontCached = Font.CreateDynamicFontFromOSFont(fontParams.fonts, fontParams.size);
                    customFonts.Add(currentFontKey, currentFontCached);
                }
            }

            return currentFontCached;
        }
    }

    private float? currentLineHeightCached;
    internal float? CurrentLineHeight
    {
        get
        {
            if (currentLineHeightCached.HasValue)
            {
                return currentLineHeightCached;
            }

            if (currentFontKey == null)
            {
                return null;
            }

            if (customFontLineHeights.TryGetValue(currentFontKey, out var currentLineHeight))
            {
                currentLineHeightCached = currentLineHeight;
            }
            else
            {
                currentLineHeight = Text.CalcHeight("W", 999f);
                currentLineHeightCached = currentLineHeight;
                customFontLineHeights.Add(currentFontKey, currentLineHeight);
            }

            return currentLineHeightCached;
        }
    }

    private float? currentSpaceBetweenLinesCached;
    internal float? CurrentSpaceBetweenLines
    {
        get
        {
            if (currentSpaceBetweenLinesCached.HasValue)
            {
                return currentSpaceBetweenLinesCached;
            }

            if (currentFontKey == null)
            {
                return null;
            }

            if (customFontSpaceBetweenLines.TryGetValue(currentFontKey, out var currentSpaceBetweenLines))
            {
                currentSpaceBetweenLinesCached = currentSpaceBetweenLines;
            }
            else
            {
                currentSpaceBetweenLines = Text.CalcHeight("W\nW", 999f) - Text.CalcHeight("W", 999f) * 2f;
                currentSpaceBetweenLinesCached = currentSpaceBetweenLines;
                customFontSpaceBetweenLines.Add(currentFontKey, currentSpaceBetweenLines);
            }

            return currentSpaceBetweenLinesCached;
        }
    }

    private GUIStyle? currentFontStyleCached;
    internal GUIStyle? CurrentFontStyle
    {
        get
        {
            if (currentFontStyleCached != null)
            {
                return currentFontStyleCached;
            }

            if (currentFontKey == null)
            {
                return null;
            }

            if (!customFontStyles.TryGetValue(currentFontKey, out currentFontStyleCached))
            {
                var currentFont = CurrentFont;
                if (currentFont != null)
                {
                    currentFontStyleCached = new(GUI.skin.label) { font = currentFont };
                    customFontStyles.Add(currentFontKey, currentFontStyleCached);
                }
            }

            return currentFontStyleCached;
        }
    }

    private GUIStyle? currentTextFieldStyleCached;
    internal GUIStyle? CurrentTextFieldStyle
    {
        get
        {
            if (currentTextFieldStyleCached != null)
            {
                return currentTextFieldStyleCached;
            }

            if (currentFontKey == null)
            {
                return null;
            }

            if (!customTextFieldStyles.TryGetValue(currentFontKey, out currentTextFieldStyleCached))
            {
                var currentFont = CurrentFont;
                if (currentFont != null)
                {
                    currentTextFieldStyleCached = new(GUI.skin.textField)
                    {
                        font = currentFont,
                        alignment = TextAnchor.MiddleLeft,
                    };
                    customTextFieldStyles.Add(currentFontKey, currentTextFieldStyleCached);
                }
            }

            return currentTextFieldStyleCached;
        }
    }

    private GUIStyle? currentTextAreaStyleCached;
    internal GUIStyle? CurrentTextAreaStyle
    {
        get
        {
            if (currentTextAreaStyleCached != null)
            {
                return currentTextAreaStyleCached;
            }

            if (currentFontKey == null)
            {
                return null;
            }

            if (!customTextAreaStyles.TryGetValue(currentFontKey, out currentTextAreaStyleCached))
            {
                var currentFont = CurrentFont;
                if (currentFont != null)
                {
                    currentTextAreaStyleCached = new(GUI.skin.textField)
                    {
                        font = currentFont,
                        alignment = TextAnchor.UpperLeft,
                        wordWrap = true,
                    };
                    customTextAreaStyles.Add(currentFontKey, currentTextAreaStyleCached);
                }
            }

            return currentTextAreaStyleCached;
        }
    }

    private GUIStyle? currentTextAreaReadOnlyStyleCached;
    internal GUIStyle? CurrentTextAreaReadOnlyStyle
    {
        get
        {
            if (currentTextAreaReadOnlyStyleCached != null)
            {
                return currentTextAreaReadOnlyStyleCached;
            }

            if (currentFontKey == null)
            {
                return null;
            }

            if (!customTextAreaReadOnlyStyles.TryGetValue(currentFontKey, out currentTextAreaReadOnlyStyleCached))
            {
                var currentFont = CurrentFont;
                if (currentFont != null)
                {
                    currentTextAreaReadOnlyStyleCached = new(GUI.skin.textField)
                    {
                        font = currentFont,
                        alignment = TextAnchor.UpperLeft,
                        wordWrap = true,
                    };
                    currentTextAreaReadOnlyStyleCached.normal.background = null;
                    currentTextAreaReadOnlyStyleCached.active.background = null;
                    currentTextAreaReadOnlyStyleCached.onHover.background = null;
                    currentTextAreaReadOnlyStyleCached.hover.background = null;
                    currentTextAreaReadOnlyStyleCached.onFocused.background = null;
                    currentTextAreaReadOnlyStyleCached.focused.background = null;
                    customTextAreaReadOnlyStyles.Add(currentFontKey, currentTextAreaReadOnlyStyleCached);
                }
            }

            return currentTextAreaReadOnlyStyleCached;
        }
    }

    private static void LogFeatureNotEnabled()
    {
        Logger.LogError("CustomFontManager is not active. Make sure you call CustomFontManager.EnableFeature() in your mod's constructor to enable it.");
    }

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
        currentFontKey = key;
    }

    public void ClearFont()
    {
        if (!featureEnabled)
        {
            LogFeatureNotEnabled();
            return;
        }

        currentFontKey = null;
        currentFontCached = null;
        currentFontStyleCached = null;
        currentTextFieldStyleCached = null;
        currentTextAreaStyleCached = null;
        currentTextAreaReadOnlyStyleCached = null;
        currentLineHeightCached = null;
        currentSpaceBetweenLinesCached = null;
    }
}
