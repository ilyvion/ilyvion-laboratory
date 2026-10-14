// Widgets_TextFieldTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;
using ilyvion.Laboratory.UI;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class Widgets_TextFieldTests
{
    [Test]
    public static void ShouldLoseFocusWhenMouseDownOutsideRectWhileFocused() =>
        Expect.IsTrue(
            Widgets_TextField.ShouldLoseFocus(
                EventType.MouseDown,
                mouseInsideRect: false,
                controlIsFocused: true
            )
        );

    [Test]
    public static void ShouldNotLoseFocusWhenMouseDownInsideRect() =>
        Expect.IsFalse(
            Widgets_TextField.ShouldLoseFocus(
                EventType.MouseDown,
                mouseInsideRect: true,
                controlIsFocused: true
            )
        );

    [Test]
    public static void ShouldNotLoseFocusWhenControlNotFocused() =>
        Expect.IsFalse(
            Widgets_TextField.ShouldLoseFocus(
                EventType.MouseDown,
                mouseInsideRect: false,
                controlIsFocused: false
            )
        );

    [Test]
    public static void ShouldNotLoseFocusForNonMouseDownEvent() =>
        Expect.IsFalse(
            Widgets_TextField.ShouldLoseFocus(
                EventType.Layout,
                mouseInsideRect: false,
                controlIsFocused: true
            )
        );
}
