using System.Text;
using System.Text.RegularExpressions;

namespace ilyvion.Laboratory;

/// <summary>
/// Which GitHub-style admonition marker (<c>&gt; [!NOTE]</c> and friends, see
/// <see href="https://github.com/orgs/community/discussions/16925"/>) a blockquote started with, if
/// any. <see cref="None"/> marks a plain blockquote with no marker.
/// </summary>
internal enum AdmonitionKind
{
    None,
    Note,
    Tip,
    Important,
    Warning,
    Caution,
}

/// <summary>
/// One paragraph-level chunk of <see cref="MarkdownLite.ToBlocks"/> output: either a run of plain
/// text (<see cref="IsBlockquote"/> false, <see cref="Admonition"/> <see cref="AdmonitionKind.None"/>)
/// or a single blockquote's body. Rendering the block-level presentation (backgrounds, side bars,
/// icons) is left to the caller - <see cref="RichText"/> only carries inline formatting.
/// </summary>
internal readonly record struct MarkdownBlock(
    bool IsBlockquote,
    AdmonitionKind Admonition,
    string RichText
);

/// <summary>
/// Converts a small Markdown subset - bold, italic, inline code, links, backslash escapes, naked
/// URLs, and GitHub-style blockquotes/admonitions - into paragraph-level <see cref="MarkdownBlock"/>
/// values whose <see cref="MarkdownBlock.RichText"/> uses the rich text tags Unity's legacy IMGUI
/// label renderer understands (<see href="https://docs.unity3d.com/Manual/UIE-supported-tags.html"/>),
/// used to display <see cref="UpdateDef"/>/<see cref="ChangeLogParser"/> content via
/// <c>Widgets.Label</c>.
/// </summary>
internal static class MarkdownLite
{
    public const string CodeColorHex = "#8caaee";

    // Unity's rich text parser recognizes '<...>' as a tag regardless of whether it knows it, and
    // silently swallows the whole span when it doesn't - so any literal '<'/'>' left in the text
    // has to be swapped for lookalikes before it reaches Widgets.Label.
    private const char EscapedLessThan = '‹';
    private const char EscapedGreaterThan = '›';

    private static readonly Regex EscapePattern = new(
        @"\\([\\`*_\[\]()<>])",
        RegexOptions.Compiled
    );
    private static readonly Regex CodePattern = new("`([^`]+)`", RegexOptions.Compiled);
    private static readonly Regex BoldPattern = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex ItalicPattern = new(@"\*(.+?)\*|_(.+?)_", RegexOptions.Compiled);

    // The label alternation's second branch allows one level of nested "[...]" (e.g. a "[JGH]"
    // tag prefix before the link text) to appear literally inside the label without prematurely
    // closing it.
    private static readonly Regex LinkPattern = new(
        @"\[((?:[^\[\]]|\[[^\[\]]*\])+)\]\([^)]+\)",
        RegexOptions.Compiled
    );
    private static readonly Regex NakedUrlPattern = new(
        @"https?://[^\s<>]+",
        RegexOptions.Compiled
    );
    private static readonly Regex BlockquoteLinePattern = new(
        @"^[ \t]*>[ \t]?(?<text>.*)$",
        RegexOptions.Compiled
    );
    private static readonly Regex AdmonitionMarkerPattern = new(
        @"^\[!(?<type>NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Backslash-escaped characters are swapped out for a private-use placeholder before any
    // markdown/tag processing runs, so e.g. "\*" can't be mistaken for bold/italic syntax, then
    // swapped back to the literal character (or its escaped lookalike, for '<'/'>') at the end.
    private const char EscapedCharBase = '';

    // Each ApplyInlineFormatting pass swaps its output for a placeholder from this range too, so
    // e.g. a "**" pair introduced by an earlier pass's substitution (or literal markup inside a
    // code span) can't be picked up and reformatted by a later pass. Placeholders are expanded
    // back to their real text before ApplyInlineFormatting returns.
    private const char FormattedSpanBase = '';

    // Trailing punctuation (e.g. the '.' ending a sentence, or a ')' closing a parenthetical) is
    // excluded from the link itself so it still reads naturally in surrounding prose.
    private const string UrlTrailingPunctuation = ".,;:!?)]";

    private static string ReplaceNakedUrl(Match match)
    {
        var url = match.Value;
        var trailingStart = url.Length;
        while (trailingStart > 0 && UrlTrailingPunctuation.Contains(url[trailingStart - 1]))
        {
            trailingStart--;
        }

        var trailing = url[trailingStart..];
        url = url[..trailingStart];
        return $"<color={CodeColorHex}>{url}</color>{trailing}";
    }

    private static string ApplyInlineFormatting(string text)
    {
        var formattedSpans = new List<string>();

        string Protect(string replacement)
        {
            formattedSpans.Add(replacement);
            return ((char)(FormattedSpanBase + formattedSpans.Count - 1)).ToString();
        }

        text = CodePattern.Replace(
            text,
            m => Protect($"<color={CodeColorHex}>{m.Groups[1].Value}</color>")
        );
        text = BoldPattern.Replace(text, m => Protect($"<b>{m.Groups[1].Value}</b>"));
        text = ItalicPattern.Replace(
            text,
            m => Protect($"<i>{(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)}</i>")
        );
        text = LinkPattern.Replace(
            text,
            m => Protect($"<color={CodeColorHex}>{m.Groups[1].Value}</color>")
        );
        text = NakedUrlPattern.Replace(text, m => Protect(ReplaceNakedUrl(m)));

        if (formattedSpans.Count == 0)
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c < FormattedSpanBase || c >= FormattedSpanBase + formattedSpans.Count)
            {
                _ = builder.Append(c);
                continue;
            }

            _ = builder.Append(formattedSpans[c - FormattedSpanBase]);
        }

        return builder.ToString();
    }

    private static string RenderLine(string line, List<char> escapedChars)
    {
        line = line.Replace('<', EscapedLessThan).Replace('>', EscapedGreaterThan);
        line = ApplyInlineFormatting(line);
        return RestoreEscapedChars(line, escapedChars);
    }

    private static MarkdownBlock RenderBlockquote(List<string> quoteLines, List<char> escapedChars)
    {
        var admonitionMatch =
            quoteLines.Count > 0
                ? AdmonitionMarkerPattern.Match(quoteLines[0].Trim())
                : Match.Empty;

        var admonition = admonitionMatch.Success
#if v1_6_OR_GREATER
            ? Enum.Parse<AdmonitionKind>(admonitionMatch.Groups["type"].Value, true)
#else
            ? (AdmonitionKind)
                Enum.Parse(typeof(AdmonitionKind), admonitionMatch.Groups["type"].Value, true)
#endif
            : AdmonitionKind.None;

        var bodyLines = admonitionMatch.Success ? quoteLines.Skip(1) : quoteLines;
        var body = string.Join(
            "\n",
            bodyLines.Where(line => line.Length > 0).Select(line => RenderLine(line, escapedChars))
        );

        return new MarkdownBlock(true, admonition, body);
    }

    public static List<MarkdownBlock> ToBlocks(string text)
    {
        var escapedChars = new List<char>();
        text = EscapePattern.Replace(
            text,
            match =>
            {
                escapedChars.Add(match.Groups[1].Value[0]);
                return ((char)(EscapedCharBase + escapedChars.Count - 1)).ToString();
            }
        );

        var lines = Regex.Split(text, "\r\n|\r|\n");
        var blocks = new List<MarkdownBlock>();
        var textLines = new List<string>();

        void FlushText()
        {
            if (textLines.Count == 0)
            {
                return;
            }

            blocks.Add(
                new MarkdownBlock(
                    false,
                    AdmonitionKind.None,
                    string.Join("\n", textLines.Select(line => RenderLine(line, escapedChars)))
                )
            );
            textLines = [];
        }

        var lineIndex = 0;
        while (lineIndex < lines.Length)
        {
            var match = BlockquoteLinePattern.Match(lines[lineIndex]);
            if (!match.Success)
            {
                textLines.Add(lines[lineIndex]);
                lineIndex++;
                continue;
            }

            FlushText();

            var quoteLines = new List<string>();
            while (
                lineIndex < lines.Length
                && (match = BlockquoteLinePattern.Match(lines[lineIndex])).Success
            )
            {
                quoteLines.Add(match.Groups["text"].Value);
                lineIndex++;
            }

            blocks.Add(RenderBlockquote(quoteLines, escapedChars));
        }

        FlushText();
        return blocks;
    }

    /// <summary>
    /// Converts <paramref name="text"/> to a single rich text string, for callers that render a
    /// single-line label rather than a full <see cref="ToBlocks"/>-aware block layout. Any
    /// blockquote content is folded in as italicized inline text since there's no block-level
    /// decoration (side bar, icon) to draw here.
    /// </summary>
    public static string ToRichText(string text) =>
        string.Join(
            "\n",
            ToBlocks(text)
                .Select(block => block.IsBlockquote ? $"<i>{block.RichText}</i>" : block.RichText)
        );

    private static string RestoreEscapedChars(string text, List<char> escapedChars)
    {
        if (escapedChars.Count == 0)
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c < EscapedCharBase || c >= EscapedCharBase + escapedChars.Count)
            {
                _ = builder.Append(c);
                continue;
            }

            var original = escapedChars[c - EscapedCharBase];
            _ = builder.Append(
                original switch
                {
                    '<' => EscapedLessThan,
                    '>' => EscapedGreaterThan,
                    _ => original,
                }
            );
        }

        return builder.ToString();
    }
}
