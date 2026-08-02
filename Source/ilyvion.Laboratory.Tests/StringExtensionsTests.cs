// StringExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using ilyvion.Laboratory.Extensions;
using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class StringExtensionsTests
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

            Assert.That(tinySize.x).Is.Not.EqualTo(mediumSize.x);
        }
        finally
        {
            Text.Font = originalFont;
        }
    }
}
