// GUIScopeTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;
using ilyvion.Laboratory.UI;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class GUIScopeTests
{
    // Regression guard for commit 551ad5e: FontSizeScope used to be constructed before
    // FontScope switched Text.Font, so it captured and later restored the font size on the
    // wrong GameFont's shared style, permanently corrupting it.
    [Test]
    public static void MultipleDoesNotCorruptFontStyleWhenCombiningFontSizeAndGameFontChange()
    {
        var originalFont = Text.Font;
        try
        {
            Text.Font = GameFont.Small;
            var originalSmallSize = Text.CurFontStyle.fontSize;

            Text.Font = GameFont.Medium;
            var originalMediumSize = Text.CurFontStyle.fontSize;

            Text.Font = GameFont.Small;

            using (GUIScope.Multiple(fontSize: originalMediumSize + 7, gameFont: GameFont.Medium))
            {
                Expect.AreEqual(GameFont.Medium, Text.Font);
                Expect.AreEqual(originalMediumSize + 7, Text.CurFontStyle.fontSize);
            }

            Expect.AreEqual(GameFont.Small, Text.Font);
            Expect.AreEqual(originalSmallSize, Text.CurFontStyle.fontSize);

            Text.Font = GameFont.Medium;
            Expect.AreEqual(originalMediumSize, Text.CurFontStyle.fontSize);
        }
        finally
        {
            Text.Font = originalFont;
        }
    }

    // Regression guard for commit 551ad5e: ScrollViewScope.Dispose() used to call
    // Widgets.EndScrollView() unconditionally, so disposing the same scope more than once
    // ended the scroll view group multiple times. TryMarkDisposed is the extracted guard that
    // decides whether a given Dispose() call is the one that should end the scroll view.
    [Test]
    public static void TryMarkDisposedOnlyReportsTrueOnce()
    {
        var status = new ScrollViewStatus();

        Expect.IsTrue(ScrollViewScope.TryMarkDisposed(status));
        Expect.IsTrue(status.Disposed);

        Expect.IsFalse(ScrollViewScope.TryMarkDisposed(status));
        Expect.IsTrue(status.Disposed);
    }

    // Regression guard for commit 551ad5e: without resetting ScrollViewStatus.Disposed for a
    // new scope, reusing the same status object across scroll view calls (the normal usage
    // pattern for a persistent scroll view) left Disposed permanently true once the first scope
    // was disposed, so a later scope's own Dispose() would skip EndScrollView() entirely.
    [Test]
    public static void ResetForNewScopeAllowsDisposalToReportTrueAgain()
    {
        var status = new ScrollViewStatus();
        _ = ScrollViewScope.TryMarkDisposed(status);
        Expect.IsTrue(status.Disposed);

        ScrollViewScope.ResetForNewScope(status);
        Expect.IsFalse(status.Disposed);

        Expect.IsTrue(ScrollViewScope.TryMarkDisposed(status));
        Expect.IsTrue(status.Disposed);
    }
}
