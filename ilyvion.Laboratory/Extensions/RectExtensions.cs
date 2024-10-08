﻿// Parts of the code:
// Copyright Karel Kroeze, 2020-2020

namespace ilyvion.Laboratory.Extensions;

public static class RectExtensions
{
    public static Rect CenteredIn(this Rect inner, Rect outer, float x = 0f, float y = 0f)
    {
        inner = inner.CenteredOnXIn(outer).CenteredOnYIn(outer);
        inner.x += x;
        inner.y += y;
        return inner;
    }

    public static Rect RoundToInt(this Rect rect)
    {
        return new Rect(
            Mathf.RoundToInt(rect.xMin),
            Mathf.RoundToInt(rect.yMin),
            Mathf.RoundToInt(rect.width),
            Mathf.RoundToInt(rect.height));
    }

    public static Rect TrimLeft(this Rect rect, float amount)
    {
        rect = new(rect);
        rect.xMin += amount;
        return rect;
    }

    public static Rect TrimRight(this Rect rect, float amount)
    {
        rect = new(rect);
        rect.width -= amount;
        return rect;
    }
}
