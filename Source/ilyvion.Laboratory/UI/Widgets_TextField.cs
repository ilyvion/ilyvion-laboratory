// Widgets_TextField.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

namespace ilyvion.Laboratory.UI;

/// <summary>
/// Provides a text field that, unlike <see cref="Widgets.TextField(Rect, string)"/>, loses focus
/// when the player clicks outside of it — matching vanilla's <see cref="QuickSearchWidget"/>
/// behavior instead of leaving the field focused (and still capturing keyboard input) until
/// something else is clicked.
/// </summary>
public static class Widgets_TextField
{
    /// <param name="rect">The rectangle to draw the field in.</param>
    /// <param name="text">The current text.</param>
    /// <param name="controlName">
    /// A stable, unique name for this field's control, used to track and release its focus.
    /// </param>
    /// <param name="maxLength">Optional maximum input length.</param>
    public static string TextField(
        Rect rect,
        string text,
        string controlName,
        int? maxLength = null
    )
    {
        GUI.SetNextControlName(controlName);
        if (
            ShouldLoseFocus(
                OriginalEventUtility.EventType,
                rect.Contains(Event.current.mousePosition),
                GUI.GetNameOfFocusedControl() == controlName
            )
        )
        {
            Verse.UI.UnfocusCurrentControl();
        }

        return maxLength.HasValue
            ? Widgets.TextField(rect, text, maxLength.Value)
            : Widgets.TextField(rect, text);
    }

    internal static bool ShouldLoseFocus(
        EventType currentEventType,
        bool mouseInsideRect,
        bool controlIsFocused
    ) => currentEventType == EventType.MouseDown && !mouseInsideRect && controlIsFocused;
}
