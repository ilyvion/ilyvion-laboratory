// Copyright (c) 2023 bradson
// Copyright (c) 2024 Alexander Krivács Schrøder
// Taken from
// <https://github.com/bbradson/Adaptive-Storage-Framework/blob/main/Source/GUIScope.cs>
// and adapted to my needs

namespace ilyvion.Laboratory.UI;

public static class GUIScope
{
    private record struct WidgetGroupScope : IDisposable
    {
        public WidgetGroupScope(in Rect rect) => Widgets.BeginGroup(rect);

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                Widgets.EndGroup();
                _disposed = true;
            }
        }
    }

    private record struct TextAnchorScope : IDisposable
    {
        private readonly TextAnchor _original;

        public TextAnchorScope(TextAnchor anchor)
        {
            _original = Text.Anchor;
            Text.Anchor = anchor;
        }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                Text.Anchor = _original;
                _disposed = true;
            }
        }
    }

    private record struct WordWrapScope : IDisposable
    {
        private readonly bool _original;

        public WordWrapScope(bool wordWrap)
        {
            _original = Text.WordWrap;
            Text.WordWrap = wordWrap;
        }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                Text.WordWrap = _original;
                _disposed = true;
            }
        }
    }

    private record struct ColorScope : IDisposable
    {
        private readonly Color _original;

        public ColorScope(Color color)
        {
            _original = GUI.color;
            GUI.color = color;
        }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                GUI.color = _original;
                _disposed = true;
            }
        }
    }

    private record struct FontScope : IDisposable
    {
        private readonly GameFont _original;

        public FontScope(GameFont font)
        {
            _original = Text.Font;
            Text.Font = font;
        }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                Text.Font = _original;
                _disposed = true;
            }
        }
    }

    private record struct FontSizeScope : IDisposable
    {
        private readonly int _original;

        public FontSizeScope(int size)
        {
            var curFontStyle = Text.CurFontStyle;

            _original = curFontStyle.fontSize;
            curFontStyle.fontSize = size;
        }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                Text.CurFontStyle.fontSize = _original;
                _disposed = true;
            }
        }
    }

    private readonly record struct MultiScope : IDisposable
    {
        private readonly FontSizeScope? _fontSizeScope;
        private readonly FontScope? _fontScope;
        private readonly ColorScope? _colorScope;
        private readonly WordWrapScope? _wordWrapScope;
        private readonly TextAnchorScope? _textAnchorScope;

        public MultiScope(
            int? fontSize,
            GameFont? gameFont,
            Color? color,
            bool? wordWrap,
            TextAnchor? textAnchor)
        {
            if (fontSize.HasValue)
            {
                _fontSizeScope = new FontSizeScope(fontSize.Value);
            }
            if (gameFont.HasValue)
            {
                _fontScope = new FontScope(gameFont.Value);
            }
            if (color.HasValue)
            {
                _colorScope = new ColorScope(color.Value);
            }
            if (wordWrap.HasValue)
            {
                _wordWrapScope = new WordWrapScope(wordWrap.Value);
            }
            if (textAnchor.HasValue)
            {
                _textAnchorScope = new TextAnchorScope(textAnchor.Value);
            }
        }

        public void Dispose()
        {
            _fontSizeScope?.Dispose();
            _fontScope?.Dispose();
            _colorScope?.Dispose();
            _wordWrapScope?.Dispose();
            _textAnchorScope?.Dispose();
        }
    }

    public static IDisposable WidgetGroup(in Rect rect) =>
        new WidgetGroupScope(rect);

    public static ScrollViewScope ScrollView(
        Rect outRect,
        ScrollViewStatus scrollViewStatus,
        bool showScrollbars = true) =>
        new(
            outRect,
            scrollViewStatus ?? throw new ArgumentNullException(nameof(scrollViewStatus)),
            showScrollbars);

    public static IDisposable TextAnchor(TextAnchor textAnchor) =>
        new TextAnchorScope(textAnchor);

    public static IDisposable WordWrap(bool wordWrap) =>
        new WordWrapScope(wordWrap);

    public static IDisposable Color(Color color) =>
        new ColorScope(color);

    public static IDisposable Font(GameFont gameFont) =>
        new FontScope(gameFont);

    public static IDisposable FontSize(int fontSize) =>
        new FontSizeScope(fontSize);

    public static IDisposable Multiple(
        int? fontSize = null,
        GameFont? gameFont = null,
        Color? color = null,
        bool? wordWrap = null,
        TextAnchor? textAnchor = null)
    => new MultiScope(fontSize, gameFont, color, wordWrap, textAnchor);
}

public class ScrollViewStatus
{
    internal Vector2 Position;
    internal float Height;
}

public readonly record struct ScrollViewScope : IDisposable
{
    private readonly ScrollViewStatus _scrollViewStatus;

    private readonly float _outRectHeight;

    private readonly Rect _viewRect;
    public Rect ViewRect => _viewRect;

    public ref float Height => ref _scrollViewStatus.Height;

    public ScrollViewScope(Rect outRect, ScrollViewStatus scrollViewStatus, bool showScrollbars)
    {
        _scrollViewStatus = scrollViewStatus
            ?? throw new ArgumentNullException(nameof(scrollViewStatus));
        _outRectHeight = outRect.height;
        _viewRect = new(0f, 0f, outRect.width, Math.Max(Height, _outRectHeight));
        if (Height - 0.1f >= outRect.height)
            _viewRect.width -= 16f;

        Height = 0f;
        Widgets.BeginScrollView(outRect, ref _scrollViewStatus.Position, _viewRect, showScrollbars);
    }

    public void Dispose() => Widgets.EndScrollView();

    public bool CanCull(float entryHeight, float entryY)
        => entryY + entryHeight < _scrollViewStatus.Position.y
            || entryY > _scrollViewStatus.Position.y + _outRectHeight;
}
