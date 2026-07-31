// Parts of the code:
// Copyright Karel Kroeze, 2020-2020

using ilyvion.Laboratory.UI;

namespace ilyvion.Laboratory.Extensions;

[HotSwappable]
public static class StringExtensions
{
    private static readonly Dictionary<Pair<string, float>, (bool fits, Vector2 textSize)> _fitsCache =
        [];

    public static string Bold(this TaggedString text)
    {
        return text.Resolve().Bold();
    }

    public static string Bold(this string text)
    {
        return $"<b>{text}</b>";
    }

    public static bool Fits(this string text, float width, out Vector2 textSize)
    {
        var key = new Pair<string, float>(text, width);
        if (_fitsCache.TryGetValue(key, out var value))
        {
            textSize = value.textSize;
            return value.fits;
        }

        if (_fitsCache.Count >= 100)
        {
            _fitsCache.Clear();
        }

        using (GUIScope.WordWrap(false))
        {
            textSize = Text.CalcSize(text);
            value = (textSize.x < width, textSize);
        }

        _fitsCache.Add(key, value);
        return value.fits;
    }

    public static string Italic(this TaggedString text)
    {
        return text.Resolve().Italic();
    }

    public static string Italic(this string text)
    {
        return $"<i>{text}</i>";
    }

    public static bool CanParseAsInt(this string text)
    {
        return int.TryParse(text, out var _);
    }
}
