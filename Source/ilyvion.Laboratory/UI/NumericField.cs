// NumericField.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

namespace ilyvion.Laboratory.UI;

/// <summary>
/// A numeric value edited through <see cref="Widgets.TextFieldNumeric{T}"/>, together with the
/// text buffer behind the field. Whenever the value changes outside the field, such as when
/// settings are loaded, the buffer is rebuilt from it in the current culture.
/// </summary>
/// <typeparam name="T">The numeric type edited by the field.</typeparam>
[SinceVersion(0, 27, 0)]
public sealed class NumericField<T>
    where T : struct, IFormattable
{
    private readonly string? _format;
    private T _value;
    private T _bufferValue;
    private string _buffer;
    private bool _focusPending;

    /// <summary>Initializes a new instance holding <paramref name="initialValue"/>.</summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="format">
    /// The format string the value is rendered into the buffer with, passed to
    /// <see cref="IFormattable.ToString(string?, IFormatProvider?)"/>.
    /// </param>
    /// <param name="focusOnFirstDraw">Whether to focus the field the first time it is drawn.</param>
    public NumericField(
        T initialValue = default,
        string? format = null,
        bool focusOnFirstDraw = false
    )
    {
        _format = format;
        _value = initialValue;
        _bufferValue = initialValue;
        _buffer = FormatValue(initialValue);
        _focusPending = focusOnFirstDraw;
    }

    /// <summary>
    /// The current value, returned by reference so it can be passed to
    /// <see cref="Scribe_Values.Look{T}(ref T, string, T, bool)"/>.
    /// </summary>
    public ref T Value => ref _value;

    /// <summary>The text shown in the field.</summary>
    public string Buffer
    {
        get
        {
            SyncBuffer();
            return _buffer;
        }
    }

    /// <summary>Draws the field at <paramref name="rect"/>.</summary>
    /// <param name="rect">The rectangle to draw the field within.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    public void Draw(Rect rect, float min = 0f, float max = 1E+09f)
    {
        SyncBuffer();
        Widgets.TextFieldNumeric(rect, ref _value, ref _buffer, min, max);
        _bufferValue = _value;

        if (_focusPending)
        {
            // Widgets.TextFieldNumeric names its control itself, overriding any name set beforehand.
            GUI.FocusControl(
                "TextField"
                    + rect.y.ToString("F0", CultureInfo.CurrentCulture)
                    + rect.x.ToString("F0", CultureInfo.CurrentCulture)
            );
            _focusPending = false;
        }
    }

    /// <summary>
    /// Draws <paramref name="label"/> in the left half of <paramref name="rect"/> and the field in
    /// the right half.
    /// </summary>
    /// <param name="rect">The rectangle to draw the label and field within.</param>
    /// <param name="label">The label.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    public void DrawLabeled(Rect rect, string label, float min = 0f, float max = 1E+09f)
    {
        using (GUIScope.TextAnchor(TextAnchor.MiddleRight))
        {
            Widgets.Label(rect.LeftHalf().Rounded(), label);
        }
        Draw(rect.RightHalf().Rounded(), min, max);
    }

    /// <summary>Draws the field as the next row of <paramref name="listing"/>.</summary>
    /// <param name="listing">The listing to draw the field in.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    public void Draw(Listing_Standard listing, float min = 0f, float max = 1E+09f)
    {
        ArgumentNullException.ThrowIfNull(listing);

        Draw(listing.GetRect(Text.LineHeight), min, max);
        listing.Gap(listing.verticalSpacing);
    }

    /// <summary>Draws the labeled field as the next row of <paramref name="listing"/>.</summary>
    /// <param name="listing">The listing to draw the label and field in.</param>
    /// <param name="label">The label.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    public void DrawLabeled(
        Listing_Standard listing,
        string label,
        float min = 0f,
        float max = 1E+09f
    )
    {
        ArgumentNullException.ThrowIfNull(listing);

        DrawLabeled(listing.GetRect(Text.LineHeight), label, min, max);
        listing.Gap(listing.verticalSpacing);
    }

    private void SyncBuffer()
    {
        if (!EqualityComparer<T>.Default.Equals(_value, _bufferValue))
        {
            _buffer = FormatValue(_value);
            _bufferValue = _value;
        }
    }

    private string FormatValue(T value) => value.ToString(_format, CultureInfo.CurrentCulture);
}
