// MarkdownLiteTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class MarkdownLiteTests
{
    [Test]
    public static void ConvertsBold() =>
        Assert.That(MarkdownLite.ToRichText("**bold**")).Is.EqualTo("<b>bold</b>");

    [Test]
    public static void ConvertsAsteriskItalic() =>
        Assert.That(MarkdownLite.ToRichText("*italic*")).Is.EqualTo("<i>italic</i>");

    [Test]
    public static void ConvertsUnderscoreItalic() =>
        Assert.That(MarkdownLite.ToRichText("_italic_")).Is.EqualTo("<i>italic</i>");

    [Test]
    public static void ConvertsInlineCodeToColoredText() =>
        Assert.That(MarkdownLite.ToRichText("`code`")).Is.EqualTo("<color=#8caaee>code</color>");

    [Test]
    public static void LiteralAsterisksInsideInlineCodeAreNotReformattedAsBold() =>
        Assert
            .That(MarkdownLite.ToRichText("`**bold**`"))
            .Is.EqualTo("<color=#8caaee>**bold**</color>");

    [Test]
    public static void ConvertsLinksToPlainStyledText() =>
        Assert
            .That(MarkdownLite.ToRichText("[label](https://example.com)"))
            .Is.EqualTo("<color=#8caaee>label</color>");

    [Test]
    public static void ConvertsLinksWithABracketedTagPrefixInTheLabel() =>
        Assert
            .That(
                MarkdownLite.ToRichText(
                    "[[JGH] Colony Manager retexture](https://steamcommunity.com/sharedfiles/filedetails/?id=2603340242)"
                )
            )
            .Is.EqualTo("<color=#8caaee>[JGH] Colony Manager retexture</color>");

    [Test]
    public static void BackslashEscapesRenderTheLiteralCharacter() =>
        Assert.That(MarkdownLite.ToRichText(@"Log\*Once")).Is.EqualTo("Log*Once");

    [Test]
    public static void EscapedAsteriskDoesNotTriggerItalic() =>
        Assert.That(MarkdownLite.ToRichText(@"a \*b\* c")).Is.EqualTo("a *b* c");

    [Test]
    public static void LiteralAngleBracketsAreEscapedSoTheyRenderInsteadOfBeingSwallowed() =>
        Assert
            .That(MarkdownLite.ToRichText("<ThingDef ParentName=\"x\">"))
            .Is.EqualTo("‹ThingDef ParentName=\"x\"›");

    [Test]
    public static void EscapedAngleBracketsAlsoRenderAsTheLookalike() =>
        Assert.That(MarkdownLite.ToRichText(@"\<b\>")).Is.EqualTo("‹b›");

    [Test]
    public static void PlainTextIsUnchanged() =>
        Assert
            .That(MarkdownLite.ToRichText("plain text, nothing special."))
            .Is.EqualTo("plain text, nothing special.");

    [Test]
    public static void ConvertsNakedUrlsToStyledText() =>
        Assert
            .That(MarkdownLite.ToRichText("See https://example.com for details."))
            .Is.EqualTo("See <color=#8caaee>https://example.com</color> for details.");

    [Test]
    public static void NakedUrlTrailingPunctuationStaysOutsideTheStyledSpan() =>
        Assert
            .That(MarkdownLite.ToRichText("(https://example.com)"))
            .Is.EqualTo("(<color=#8caaee>https://example.com</color>)");

    [Test]
    public static void ToBlocksSplitsAGitHubAdmonitionOutAsItsOwnBlockquoteBlock()
    {
        var blocks = MarkdownLite.ToBlocks(
            "> [!IMPORTANT]\n> This release requires an update to ilyvion's Laboratory!"
        );

        Assert.ThatCollection(blocks).Has.Count(1);
        Assert.That(blocks[0].IsBlockquote).Is.True();
        Assert.That(blocks[0].Admonition).Is.EqualTo(AdmonitionKind.Important);
        Assert
            .That(blocks[0].RichText)
            .Is.EqualTo("This release requires an update to ilyvion's Laboratory!");
    }

    [Test]
    public static void ToBlocksAdmonitionBodyStillGetsInlineFormatting()
    {
        var blocks = MarkdownLite.ToBlocks("> [!NOTE]\n> See **the docs**.");

        Assert.ThatCollection(blocks).Has.Count(1);
        Assert.That(blocks[0].Admonition).Is.EqualTo(AdmonitionKind.Note);
        Assert.That(blocks[0].RichText).Is.EqualTo("See <b>the docs</b>.");
    }

    [Test]
    public static void ToBlocksPlainBlockquoteWithoutAnAdmonitionMarkerHasNoneAdmonition()
    {
        var blocks = MarkdownLite.ToBlocks("> Just a quote.");

        Assert.ThatCollection(blocks).Has.Count(1);
        Assert.That(blocks[0].IsBlockquote).Is.True();
        Assert.That(blocks[0].Admonition).Is.EqualTo(AdmonitionKind.None);
        Assert.That(blocks[0].RichText).Is.EqualTo("Just a quote.");
    }

    [Test]
    public static void ToBlocksTextAroundABlockquoteBecomesSeparateTextBlocks()
    {
        var blocks = MarkdownLite.ToBlocks("Before\n> [!WARNING]\n> Careful.\nAfter");

        Assert.ThatCollection(blocks).Has.Count(3);
        Assert.That(blocks[0].IsBlockquote).Is.False();
        Assert.That(blocks[0].RichText).Is.EqualTo("Before");
        Assert.That(blocks[1].IsBlockquote).Is.True();
        Assert.That(blocks[1].Admonition).Is.EqualTo(AdmonitionKind.Warning);
        Assert.That(blocks[1].RichText).Is.EqualTo("Careful.");
        Assert.That(blocks[2].IsBlockquote).Is.False();
        Assert.That(blocks[2].RichText).Is.EqualTo("After");
    }

    [Test]
    public static void ToRichTextFoldsABlockquoteInAsItalicizedInlineText() =>
        Assert
            .That(MarkdownLite.ToRichText("> [!IMPORTANT]\n> Careful."))
            .Is.EqualTo("<i>Careful.</i>");
}
