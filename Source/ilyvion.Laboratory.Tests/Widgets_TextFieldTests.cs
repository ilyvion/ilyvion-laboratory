// Widgets_TextFieldTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using ilyvion.Laboratory.UI;
using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class Widgets_TextFieldTests
{
    [Test]
    public static void ShouldLoseFocusWhenMouseDownOutsideRectWhileFocused() =>
        Assert
            .That(
                Widgets_TextField.ShouldLoseFocus(
                    EventType.MouseDown,
                    mouseInsideRect: false,
                    controlIsFocused: true
                )
            )
            .Is.True();

    [Test]
    public static void ShouldNotLoseFocusWhenMouseDownInsideRect() =>
        Assert
            .That(
                Widgets_TextField.ShouldLoseFocus(
                    EventType.MouseDown,
                    mouseInsideRect: true,
                    controlIsFocused: true
                )
            )
            .Is.False();

    [Test]
    public static void ShouldNotLoseFocusWhenControlNotFocused() =>
        Assert
            .That(
                Widgets_TextField.ShouldLoseFocus(
                    EventType.MouseDown,
                    mouseInsideRect: false,
                    controlIsFocused: false
                )
            )
            .Is.False();

    [Test]
    public static void ShouldNotLoseFocusForNonMouseDownEvent() =>
        Assert
            .That(
                Widgets_TextField.ShouldLoseFocus(
                    EventType.Layout,
                    mouseInsideRect: false,
                    controlIsFocused: true
                )
            )
            .Is.False();
}
