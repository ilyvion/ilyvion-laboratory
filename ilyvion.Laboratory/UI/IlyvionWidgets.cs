namespace ilyvion.Laboratory.UI;

public static class IlyvionWidgets
{
    public static void DrawDashedLine(
        Vector2 start,
        Vector2 end,
        float rawDashLength,
        Color color,
        float width,
        float dashSpacing = 2f)
    {
        var lineLength = (end - start).magnitude;
        float rawDashCount = lineLength / (rawDashLength + dashSpacing);

        bool okayLength = rawDashCount >= 4 || (rawDashLength / 2) < 1;
        while (!okayLength)
        {
            rawDashCount = lineLength / (rawDashLength + dashSpacing);
            if (rawDashCount >= 4 || (rawDashLength / 2) < 1)
            {
                okayLength = true;
            }
            else
            {
                rawDashLength /= 2;
            }
        }

        var dashCount = Mathf.Floor(rawDashCount);
        var dashLength = (lineLength - (dashCount * dashSpacing)) / dashCount;
        var dashSpaceLength = dashLength + dashSpacing;

        var rise = end.y - start.y;
        var risePerUnit = rise / lineLength;
        var run = end.x - start.x;
        var runPerUnit = run / lineLength;

        for (int segment = 0; segment < dashCount; segment++)
        {
            var segmentX = start.x + segment * dashSpaceLength * runPerUnit;
            var segmentY = start.y + segment * dashSpaceLength * risePerUnit;
            var segmentStart = new Vector2(segmentX, segmentY);
            var segmentEnd = new Vector2(segmentX + dashLength * runPerUnit, segmentY + dashLength * risePerUnit);

            Widgets.DrawLine(segmentStart, segmentEnd, color, width);
        }
    }

    public static bool DisableableButtonText(
        Rect rect,
        string label,
        bool drawBackground = true,
        bool doMouseoverSound = true,
        Color? textColor = null,
        bool enabled = true,
        TextAnchor? overrideTextAnchor = null)
    {
        Color realizedTextColor = textColor ?? Widgets.NormalOptionColor;

        var color = GUI.color;
        if (!enabled)
        {
            if (drawBackground)
            {
                GUI.color = Color.gray;
                Texture2D atlas = Widgets.ButtonBGAtlas;
                Widgets.DrawAtlas(rect, atlas);
                GUI.color = color;
            }
            else
            {
                GUI.color = realizedTextColor;
                if (Mouse.IsOver(rect))
                {
                    GUI.color = Widgets.MouseoverOptionColor;
                }
            }

            TextAnchor anchor = Text.Anchor;
            if (overrideTextAnchor.HasValue)
            {
                Text.Anchor = overrideTextAnchor.Value;
            }
            else if (drawBackground)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleLeft;
            }
            bool wordWrap = Text.WordWrap;
            if (rect.height < Text.LineHeight * 2f)
            {
                Text.WordWrap = false;
            }
            Widgets.Label(rect, label);
            Text.Anchor = anchor;
            GUI.color = color;
            Text.WordWrap = wordWrap;

            GUI.color = color;

            return false;
        }
        else
        {
            return Widgets.ButtonText(
                rect,
                label,
                drawBackground,
                doMouseoverSound,
                realizedTextColor,
                enabled
#if !v1_3
                , overrideTextAnchor
#endif
            );
        }
    }
}

#if v1_3
public static class ListingStandardBackwardsCompatible
{
    public static float SliderLabeled(this Listing_Standard listing, string label, float val, float min, float max, float labelPct = 0.5f, string? tooltip = null)
    {
        if (listing == null)
        {
            throw new ArgumentNullException(nameof(listing));
        }

        Rect rect = listing.GetRect(30f);
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect.LeftPart(labelPct), label);
        if (tooltip != null)
        {
            TooltipHandler.TipRegion(rect.LeftPart(labelPct), tooltip);
        }
        Text.Anchor = TextAnchor.UpperLeft;
        float result = Widgets.HorizontalSlider(rect.RightPart(1f - labelPct), val, min, max, middleAlignment: true);
        listing.Gap(listing.verticalSpacing);
        return result;
    }
}
#endif
