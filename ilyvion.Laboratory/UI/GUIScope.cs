// Copyright (c) 2023 bradson
// Copyright (c) 2024 Alexander Krivács Schrøder
// Taken from
// <https://github.com/bbradson/Adaptive-Storage-Framework/blob/main/Source/GUIScope.cs>
// and adapted to my needs

namespace ilyvion.Laboratory.UI;

public static class GUIScope
{
    private readonly record struct WidgetGroupScope : IDisposable
    {
        public WidgetGroupScope(in Rect rect) => Widgets.BeginGroup(rect);

        public void Dispose() => Widgets.EndGroup();
    }

    private readonly record struct ScrollViewScope : IDisposable
    {
        private readonly ScrollViewStatus _scrollViewStatus;

        private readonly float _outRectHeight;

        public readonly Rect Rect;

        public ref float Height => ref _scrollViewStatus.Height;

        public ScrollViewScope(Rect outRect, ScrollViewStatus scrollViewStatus, bool showScrollbars)
        {
            _scrollViewStatus = scrollViewStatus;
            _outRectHeight = outRect.height;
            Rect = new(0f, 0f, outRect.width, Math.Max(Height, _outRectHeight));
            if (Height - 0.1f >= outRect.height)
                Rect.width -= 16f;

            Height = 0f;
            Widgets.BeginScrollView(outRect, ref _scrollViewStatus.Position, Rect, showScrollbars);
        }

        public void Dispose() => Widgets.EndScrollView();

        public bool CanCull(float entryHeight, float entryY)
            => entryY + entryHeight < _scrollViewStatus.Position.y
                || entryY > _scrollViewStatus.Position.y + _outRectHeight;
    }

    private readonly record struct TextAnchorScope : IDisposable
    {
        private readonly UnityEngine.TextAnchor _default;

        public TextAnchorScope(UnityEngine.TextAnchor anchor)
        {
            _default = Text.Anchor;
            Text.Anchor = anchor;
        }

        public void Dispose() => Text.Anchor = _default;
    }

    private readonly record struct WordWrapScope : IDisposable
    {
        private readonly bool _default;

        public WordWrapScope(bool wordWrap)
        {
            _default = Text.WordWrap;
            Text.WordWrap = wordWrap;
        }

        public void Dispose() => Text.WordWrap = _default;
    }

    private readonly record struct ColorScope : IDisposable
    {
        private readonly UnityEngine.Color _default;

        public ColorScope(UnityEngine.Color color)
        {
            _default = GUI.color;
            GUI.color = color;
        }

        public void Dispose() => GUI.color = _default;
    }

    private readonly record struct FontScope : IDisposable
    {
        private readonly GameFont _default;

        public FontScope(GameFont font)
        {
            _default = Text.Font;
            Text.Font = font;
        }

        public void Dispose() => Text.Font = _default;
    }

    private readonly record struct FontSizeScope : IDisposable
    {
        private readonly int _default;

        public FontSizeScope(int size)
        {
            var curFontStyle = Text.CurFontStyle;

            _default = curFontStyle.fontSize;
            curFontStyle.fontSize = size;
        }

        public void Dispose() => Text.CurFontStyle.fontSize = _default;
    }

    public static IDisposable WidgetGroup(in Rect rect) =>
        new WidgetGroupScope(rect);

    public static IDisposable ScrollView(
        Rect outRect,
        ScrollViewStatus scrollViewStatus,
        bool showScrollbars = true) =>
        new ScrollViewScope(
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
}

public class ScrollViewStatus
{
    internal Vector2 Position;
    internal float Height;
}
