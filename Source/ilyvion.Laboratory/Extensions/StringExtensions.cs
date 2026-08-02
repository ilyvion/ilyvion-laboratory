// Parts of the code:
// Copyright Karel Kroeze, 2020-2020

using ilyvion.Laboratory.Collections;
using ilyvion.Laboratory.UI;

namespace ilyvion.Laboratory.Extensions;

[HotSwappable]
public static class StringExtensions
{
    private static readonly LruCache<
        (string text, float width, GameFont font),
        (bool fits, Vector2 textSize)
    > _fitsCache = new(100);

    public static string Bold(this TaggedString text) => text.Resolve().Bold();

    public static string Bold(this string text) => $"<b>{text}</b>";

    public static bool Fits(this string text, float width, out Vector2 textSize)
    {
        var key = (text, width, Text.Font);
        if (_fitsCache.TryGetValue(key, out var value))
        {
            textSize = value.textSize;
            return value.fits;
        }

        using (GUIScope.WordWrap(false))
        {
            textSize = Text.CalcSize(text);
            value = (textSize.x < width, textSize);
        }

        _fitsCache.Set(key, value);
        return value.fits;
    }

    public static string Italic(this TaggedString text) => text.Resolve().Italic();

    public static string Italic(this string text) => $"<i>{text}</i>";

    public static bool CanParseAsInt(this string text) => int.TryParse(text, out var _);
}
