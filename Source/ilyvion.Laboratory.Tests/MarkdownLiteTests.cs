// MarkdownLiteTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class MarkdownLiteTests
{
    [Test]
    public static void ConvertsBold() =>
        Expect.AreEqual("<b>bold</b>", MarkdownLite.ToRichText("**bold**"));

    [Test]
    public static void ConvertsAsteriskItalic() =>
        Expect.AreEqual("<i>italic</i>", MarkdownLite.ToRichText("*italic*"));

    [Test]
    public static void ConvertsUnderscoreItalic() =>
        Expect.AreEqual("<i>italic</i>", MarkdownLite.ToRichText("_italic_"));

    [Test]
    public static void ConvertsInlineCodeToColoredText() =>
        Expect.AreEqual("<color=#8caaee>code</color>", MarkdownLite.ToRichText("`code`"));

    [Test]
    public static void LiteralAsterisksInsideInlineCodeAreNotReformattedAsBold() =>
        Expect.AreEqual("<color=#8caaee>**bold**</color>", MarkdownLite.ToRichText("`**bold**`"));

    [Test]
    public static void ConvertsLinksToPlainStyledText() =>
        Expect.AreEqual(
            "<color=#8caaee>label</color>",
            MarkdownLite.ToRichText("[label](https://example.com)")
        );

    [Test]
    public static void ConvertsLinksWithABracketedTagPrefixInTheLabel() =>
        Expect.AreEqual(
            "<color=#8caaee>[JGH] Colony Manager retexture</color>",
            MarkdownLite.ToRichText(
                "[[JGH] Colony Manager retexture](https://steamcommunity.com/sharedfiles/filedetails/?id=2603340242)"
            )
        );

    [Test]
    public static void BackslashEscapesRenderTheLiteralCharacter() =>
        Expect.AreEqual("Log*Once", MarkdownLite.ToRichText(@"Log\*Once"));

    [Test]
    public static void EscapedAsteriskDoesNotTriggerItalic() =>
        Expect.AreEqual("a *b* c", MarkdownLite.ToRichText(@"a \*b\* c"));

    [Test]
    public static void LiteralAngleBracketsAreEscapedSoTheyRenderInsteadOfBeingSwallowed() =>
        Expect.AreEqual(
            "‹ThingDef ParentName=\"x\"›",
            MarkdownLite.ToRichText("<ThingDef ParentName=\"x\">")
        );

    [Test]
    public static void EscapedAngleBracketsAlsoRenderAsTheLookalike() =>
        Expect.AreEqual("‹b›", MarkdownLite.ToRichText(@"\<b\>"));

    [Test]
    public static void PlainTextIsUnchanged() =>
        Expect.AreEqual(
            "plain text, nothing special.",
            MarkdownLite.ToRichText("plain text, nothing special.")
        );

    [Test]
    public static void ConvertsNakedUrlsToStyledText() =>
        Expect.AreEqual(
            "See <color=#8caaee>https://example.com</color> for details.",
            MarkdownLite.ToRichText("See https://example.com for details.")
        );

    [Test]
    public static void NakedUrlTrailingPunctuationStaysOutsideTheStyledSpan() =>
        Expect.AreEqual(
            "(<color=#8caaee>https://example.com</color>)",
            MarkdownLite.ToRichText("(https://example.com)")
        );

    [Test]
    public static void ToBlocksSplitsAGitHubAdmonitionOutAsItsOwnBlockquoteBlock()
    {
        var blocks = MarkdownLite.ToBlocks(
            "> [!IMPORTANT]\n> This release requires an update to ilyvion's Laboratory!"
        );

        Expect.AreEqual(1, blocks.Count);
        Expect.IsTrue(blocks[0].IsBlockquote);
        Expect.AreEqual(AdmonitionKind.Important, blocks[0].Admonition);
        Expect.AreEqual(
            "This release requires an update to ilyvion's Laboratory!",
            blocks[0].RichText
        );
    }

    [Test]
    public static void ToBlocksAdmonitionBodyStillGetsInlineFormatting()
    {
        var blocks = MarkdownLite.ToBlocks("> [!NOTE]\n> See **the docs**.");

        Expect.AreEqual(1, blocks.Count);
        Expect.AreEqual(AdmonitionKind.Note, blocks[0].Admonition);
        Expect.AreEqual("See <b>the docs</b>.", blocks[0].RichText);
    }

    [Test]
    public static void ToBlocksPlainBlockquoteWithoutAnAdmonitionMarkerHasNoneAdmonition()
    {
        var blocks = MarkdownLite.ToBlocks("> Just a quote.");

        Expect.AreEqual(1, blocks.Count);
        Expect.IsTrue(blocks[0].IsBlockquote);
        Expect.AreEqual(AdmonitionKind.None, blocks[0].Admonition);
        Expect.AreEqual("Just a quote.", blocks[0].RichText);
    }

    [Test]
    public static void ToBlocksTextAroundABlockquoteBecomesSeparateTextBlocks()
    {
        var blocks = MarkdownLite.ToBlocks("Before\n> [!WARNING]\n> Careful.\nAfter");

        Expect.AreEqual(3, blocks.Count);
        Expect.IsFalse(blocks[0].IsBlockquote);
        Expect.AreEqual("Before", blocks[0].RichText);
        Expect.IsTrue(blocks[1].IsBlockquote);
        Expect.AreEqual(AdmonitionKind.Warning, blocks[1].Admonition);
        Expect.AreEqual("Careful.", blocks[1].RichText);
        Expect.IsFalse(blocks[2].IsBlockquote);
        Expect.AreEqual("After", blocks[2].RichText);
    }

    [Test]
    public static void ToRichTextFoldsABlockquoteInAsItalicizedInlineText() =>
        Expect.AreEqual("<i>Careful.</i>", MarkdownLite.ToRichText("> [!IMPORTANT]\n> Careful."));
}
