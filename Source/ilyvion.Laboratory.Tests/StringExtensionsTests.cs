// StringExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;
using ilyvion.Laboratory.Extensions;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class StringExtensionsTests
{
    // Regression guard for BUG-25: the Fits cache used to be keyed only on (text, width),
    // ignoring Text.Font, so a size measured under one GameFont was served back for every
    // other GameFont as long as the text/width matched.
    [Test]
    public static void FitsDoesNotReuseCachedSizeAcrossDifferentFonts()
    {
        var originalFont = Text.Font;
        try
        {
            // Use a text/width pair unlikely to collide with cache entries from other tests.
            const string text =
                "StringExtensionsTests.FitsDoesNotReuseCachedSizeAcrossDifferentFonts";
            const float width = 12345f;

            Text.Font = GameFont.Tiny;
            _ = text.Fits(width, out var tinySize);

            Text.Font = GameFont.Medium;
            _ = text.Fits(width, out var mediumSize);

            Expect.AreNotEqual(mediumSize.x, tinySize.x);
        }
        finally
        {
            Text.Font = originalFont;
        }
    }
}
