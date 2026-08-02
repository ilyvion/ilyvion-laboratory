// GUIScopeTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using ilyvion.Laboratory.UI;
using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class GUIScopeTests
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
                Assert.That(Text.Font).Is.EqualTo(GameFont.Medium);
                Assert.That(Text.CurFontStyle.fontSize).Is.EqualTo(originalMediumSize + 7);
            }

            Assert.That(Text.Font).Is.EqualTo(GameFont.Small);
            Assert.That(Text.CurFontStyle.fontSize).Is.EqualTo(originalSmallSize);

            Text.Font = GameFont.Medium;
            Assert.That(Text.CurFontStyle.fontSize).Is.EqualTo(originalMediumSize);
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

        Assert.That(ScrollViewScope.TryMarkDisposed(status)).Is.True();
        Assert.That(status.Disposed).Is.True();

        Assert.That(ScrollViewScope.TryMarkDisposed(status)).Is.False();
        Assert.That(status.Disposed).Is.True();
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
        Assert.That(status.Disposed).Is.True();

        ScrollViewScope.ResetForNewScope(status);
        Assert.That(status.Disposed).Is.False();

        Assert.That(ScrollViewScope.TryMarkDisposed(status)).Is.True();
        Assert.That(status.Disposed).Is.True();
    }
}
