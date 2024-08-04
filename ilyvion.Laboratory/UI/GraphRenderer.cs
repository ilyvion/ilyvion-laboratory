// Parts of the code:
// Copyright Karel Kroeze, 2020-2020

#if !v1_3 && !v1_4
using LudeonTK;
#endif

namespace ilyvion.Laboratory.UI;

[HotSwappable]
public class GraphRenderer
{
    private const float Spacing = 6f;
    public Color BackgroundColour { get; set; } = new(0f, 0f, 0f, .2f);

    public string NoDataLabel { get; set; } = "ilyvion.Laboratory.Graph.NoData".Translate();
    public string LegendLabel { get; set; } = "ilyvion.Laboratory.Graph.Legend".Translate();

    public bool Interactive { get; set; } = true;

#pragma warning disable CA1819
    public GraphSeries[] Series { get; set; }
    public bool[] ShownSeries { get; set; }
#pragma warning restore CA1819

    /// <summary>
    /// How many points should fit on the graph. If this is null,
    /// the size of the data parameter is used to decide instead.
    /// </summary>
    public int? MaxEntries { get; set; }

    public bool DrawInlineLegend { get; set; } = true;
    public float LegendLineLength { get; set; } = 30f;

    public bool DrawTargetLine { get; set; } = true;

    public int Breaks { get; set; } = 4;

    public string YAxisUnitLabel { get; set; } = string.Empty;

    public Texture2D MouseOverIndicatorTexture { get; set; } = Resources.GraphDot;

    public GraphRenderer(GraphSeries[] series)
    {
        Series = series;
        ShownSeries = series.Select(_ => true).ToArray();
    }

    private float yAxisLabelsMaxWidth;
    public void DrawGraph(
        in Rect rect,
        in int[][] data,
        in int[]?[]? targetData)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        if (DrawTargetLine && targetData == null)
        {
            throw new ArgumentNullException(nameof(targetData));
        }

        var plotRect = rect.ContractedBy(Spacing);
        plotRect.xMin += yAxisLabelsMaxWidth;

        if (data.Length == 0)
        {
            Widgets.DrawRectFast(plotRect, BackgroundColour);
            using var _m = GUIScope.Multiple(
                textAnchor: TextAnchor.MiddleCenter,
                color: Color.grey);
            Widgets.Label(rect, NoDataLabel);
            return;
        }

        var entries = data[0].Length;
        if (MaxEntries.HasValue && entries > MaxEntries.Value)
        {
            throw new GraphException($"{nameof(data)} has more entries ({entries}) than {nameof(MaxEntries)} allows ({MaxEntries})");
        }

#pragma warning disable CA1062 
        if (DrawTargetLine && targetData!.Length != data.Length)
        {
            throw new GraphException($"{nameof(data)}'s length must match {nameof(targetData)}'s length");
        }
#pragma warning restore CA1062 

        var visibleSeries = Series.Where(s => !s.Hidden).Select((series, index) => (index, series, shown: ShownSeries[index])).ToArray();
        var visibleIndexes = visibleSeries.Select(s => s.index).ToArray();

        if (visibleSeries.Length != data.Length)
        {
            throw new GraphException($"{nameof(data)}'s length must match the number of non-hidden {nameof(Series)}");
        }

        if (MaxEntries.HasValue)
        {
            entries = MaxEntries.Value;
        }

        plotRect.xMin += Spacing;

        Widgets.DrawRectFast(plotRect, BackgroundColour);

        using var plotLegendGroup = GUIScope.WidgetGroup(plotRect);
        plotRect = plotRect.AtZero();

        var rowHeight = Text.LineHeightOf(GameFont.Tiny);
        var legendRect = new Rect(Vector2.zero, new(plotRect.width, rowHeight));
        if (DrawInlineLegend)
        {
            using var _f = GUIScope.Font(GameFont.Tiny);

            Widgets.Label(legendRect, LegendLabel + ":");
            legendRect.y += rowHeight;

            for (int i = 0; i < visibleSeries.Length; i++)
            {
                GraphSeries series = visibleSeries[i].series;
                bool isShown = visibleSeries[i].shown;
                int index = visibleSeries[i].index;

                using (GUIScope.Color(isShown ? series.Color : series.MutedColor))
                {
                    Widgets.DrawLineHorizontal(
                        legendRect.x,
                        legendRect.y + rowHeight / 2f,
                        LegendLineLength);

                    legendRect.xMin += LegendLineLength + Spacing;
                    legendRect.y += 1f;
                }

                using (GUIScope.Color(isShown ? Color.white : Color.gray))
                {
                    Widgets.Label(legendRect, series.Label);

                    legendRect.xMin -= LegendLineLength + Spacing;
                }

                if (Interactive)
                {
                    var tooltip = "ilyvion.Laboratory.Graph.ClickToEnable"
                        .Translate(isShown
                            ? "ilyvion.Laboratory.Graph.Hide".Translate()
                            : "ilyvion.Laboratory.Graph.Show".Translate(),
                            series.Label.UncapitalizeFirst());
                    TooltipHandler.TipRegion(legendRect, tooltip);
                    Widgets.DrawHighlightIfMouseover(legendRect);
                    if (Widgets.ButtonInvisible(legendRect))
                    {
                        if (Event.current.button == 0)
                        {
                            ShownSeries[index] = !ShownSeries[index];
                        }
                        else if (Event.current.button == 1)
                        {
                            for (int j = 0; j < ShownSeries.Length; j++)
                            {
                                ShownSeries[j] = false;
                            }
                            ShownSeries[index] = true;
                        }
                    }
                }

                legendRect.y += rowHeight - 1f;
            }
        }

        plotRect.yMin += legendRect.yMax;
        using var plotGroup = GUIScope.WidgetGroup(plotRect);
        plotRect = plotRect.AtZero();

        var shownSeries = visibleSeries.Where((_, i) => visibleSeries[i].shown).Select(s => s.series).ToArray();
        var shownData = data.Where((_, i) => visibleSeries[i].shown).ToArray();
        var shownTargetData = targetData.Where((_, i) => visibleSeries[i].shown).ToArray();

        if (shownData.Length == 0)
        {
            return;
        }

        int max;
        if (DrawTargetLine)
        {
            max = Utils.CeilToPrecision(shownData.Concat(shownTargetData.Where(t => t != null)).Max(s => s.Max()));
        }
        else
        {
            max = Utils.CeilToPrecision(shownData.Max(s => s.Max()));
        }

        var plotWidth = plotRect.width;
        var plotHeight = plotRect.height;
        var widthUnit = plotWidth / (entries - 1);
        var heightUnit = plotHeight / Math.Max(max, 2);
        var breakInterval = (float)Math.Max(max, 2) / (Breaks + 1);
        var breakUnit = heightUnit * breakInterval;

        for (int i = 0; i < shownData.Length; i++)
        {
            GraphSeries series = shownSeries[i];
            int[] seriesData = shownData[i];
            int[]? seriesTargetData = shownTargetData[i];
            PlotData(series, seriesData, plotRect, widthUnit, heightUnit);
            if (DrawTargetLine && seriesTargetData != null)
            {
                PlotTarget(series, seriesTargetData, plotRect, widthUnit, heightUnit);
            }
        }

        if (Interactive && Mouse.IsOver(plotRect))
        {
            DoMouseOver(plotRect, shownSeries, shownData, shownTargetData, widthUnit, heightUnit);
        }

        plotGroup.Dispose();
        plotLegendGroup.Dispose();

        using var axisGroup = GUIScope.WidgetGroup(rect);

        using (GUIScope.Multiple(textAnchor: TextAnchor.MiddleRight, gameFont: GameFont.Tiny))
        {
            // draw ticks + labels
            var labelMaxWidth = 0f;
            for (var i = 0; i <= Breaks + 1; i++)
            {
                string label = Utils.FormatCount(i * breakInterval, YAxisUnitLabel);
                labelMaxWidth = Math.Max(labelMaxWidth, Text.CalcSize(label).x);
                if (i != 0)
                {
                    Widgets.DrawLineHorizontal(yAxisLabelsMaxWidth + Spacing, plotRect.height - i * breakUnit + legendRect.yMax + Spacing, Spacing);
                }
                else
                {
                    Widgets.DrawLineHorizontal(yAxisLabelsMaxWidth + Spacing, plotRect.height - i * breakUnit + legendRect.yMax + Spacing - 1f, Spacing);
                }
                Rect labRect;
                if (i != 0)
                {
                    labRect = new Rect(0f, plotRect.height - i * breakUnit - 4f + legendRect.yMax + Spacing, yAxisLabelsMaxWidth, 20f);
                }
                else
                {
                    labRect = new Rect(0f, plotRect.height - i * breakUnit - 6f + legendRect.yMax, yAxisLabelsMaxWidth, 20f);
                }

                Widgets.Label(labRect, label);
            }
            yAxisLabelsMaxWidth = labelMaxWidth + Spacing;
        }
    }

    private static void PlotData(
        GraphSeries series,
        int[] data,
        Rect canvas,
        float widthUnit,
        float heightUnit)
    {
        if (data.Length > 1)
        {
            Vector2? lastEnd = null;
            for (var i = 0; i < data.Length - 1; i++) // line segments, so up till n-1
            {
                var start = lastEnd ?? new Vector2(widthUnit * i, canvas.height - heightUnit * data[i]);
                var end = new Vector2(Mathf.Round(widthUnit * (i + 1)), Mathf.Round(canvas.height - heightUnit * data[i + 1]));
                Widgets.DrawLine(start, end, series.Color, 1f);

                lastEnd = end;
            }
        }
    }

    private static void PlotTarget(
        GraphSeries series,
        int[] targetData,
        Rect canvas,
        float widthUnit,
        float heightUnit)
    {
        int? currentValue = null;
        int? currentValueStartIndex = null;
        if (targetData.Length > 1)
        {
            for (var i = 0; i < targetData.Length; i++) // line segments, so up till n-1
            {
                if (currentValue == null)
                {
                    currentValue = targetData[i];
                    currentValueStartIndex = i;
                    continue;
                }
                else if (currentValue != targetData[i] || i == targetData.Length - 1)
                {
                    var start = new Vector2(
                        widthUnit * currentValueStartIndex!.Value,
                        canvas.height - heightUnit * currentValue!.Value);
                    var end = new Vector2(
                        widthUnit * (i) - 1,
                        canvas.height - heightUnit * currentValue!.Value);

                    IlyvionWidgets.DrawDashedLine(start, end, 10f, series.MutedColor, 1.5f);

                    currentValue = targetData[i];
                    currentValueStartIndex = i;
                }
            }
        }
    }

    private void DoMouseOver(
        Rect plot,
        GraphSeries[] shownSeries,
        int[][] shownData,
        int[]?[] shownTargetData,
        float widthUnit,
        float heightUnit)
    {
        var mousePosition = Event.current.mousePosition;
        var unitPosition = new Vector2(mousePosition.x / widthUnit, (plot.height - mousePosition.y) / heightUnit);

        int unitXPosition = (int)Mathf.Round(unitPosition.x);

        var distances = shownData
            .Where(data => unitXPosition < data.Length)
            .Select(data => Math.Abs(data[unitXPosition] - unitPosition.y))
            .Concat(DrawTargetLine
                ? shownTargetData
                    .Where(target => unitXPosition < (target?.Length ?? shownData.Max(d => d.Length)))
                    .Select(target => Math.Abs((target != null ? target[unitXPosition] : short.MaxValue) - unitPosition.y))
                : [])
            .ToArray();

        Log.Warning("" + string.Join(", ", distances));

        // get the minimum index
        float min = int.MaxValue;
        var minIndex = 0;
        for (var i = distances.Length - 1; i >= 0; i--)
        {
            if (distances[i] < min && (i < shownData.Length || shownTargetData[i % shownData.Length] != null))
            {
                minIndex = i;
                min = distances[i];
            }
        }

        var useValue = minIndex < shownData.Length;

        var closestSeries = shownSeries[minIndex % shownData.Length];
        var closestData = shownData[minIndex % shownData.Length];
        var closestTargetData = shownTargetData[minIndex % shownData.Length];

        if (unitXPosition < closestData.Length)
        {
            var valueAt = useValue
                ? closestData[unitXPosition]
                : closestTargetData![unitXPosition];
            var realpos = new Vector2(
                unitXPosition * widthUnit,
                plot.height - Math.Max(0, valueAt) * heightUnit);
            var mouseOverIndicatorWidth = MouseOverIndicatorTexture.width;
            var mouseOverIndicatorHeight = MouseOverIndicatorTexture.height;
            var mouseOverIndicatorRect = new Rect(
                realpos.x - mouseOverIndicatorWidth / 2f,
                realpos.y - mouseOverIndicatorHeight / 2f,
                mouseOverIndicatorWidth,
                mouseOverIndicatorHeight);

            var distance = (realpos - mousePosition).magnitude;

            if (IlyvionDebugViewSettings.DrawUIHelpers)
            {
                var mousePosRect = new Rect(
                    mousePosition.x - Resources.GraphDot.width / 2f,
                    mousePosition.y - Resources.GraphDot.height / 2f,
                    Resources.GraphDot.width,
                    Resources.GraphDot.height);
                GUI.DrawTexture(mousePosRect, Resources.GraphDot);

                var labelRect = new Rect()
                {
                    x = mousePosRect.xMax,
                    y = mousePosRect.yMax + 10f,
                    width = plot.width,
                    height = 2 * Text.LineHeight
                };
                Widgets.Label(labelRect, "" + unitPosition + "\n" + distance);

                Widgets.DrawLine(mousePosition, realpos, Color.white, 1f);
            }

            if (distance < 100)
            {
                using (GUIScope.Color(useValue ? closestSeries.Color : closestSeries.MutedColor))
                {
                    GUI.DrawTexture(mouseOverIndicatorRect, MouseOverIndicatorTexture);
                }

                var tippos = realpos + new Vector2(Spacing, Spacing);
                var tip = useValue
                    ? "ilyvion.Laboratory.Graph.ValueTooltip".Translate(
                        closestSeries.Label,
                        Utils.FormatCount(closestData[unitXPosition], closestSeries.UnitLabel ?? YAxisUnitLabel))
                    : "ilyvion.Laboratory.Graph.TargetTooltip".Translate(
                        closestSeries.Label,
                        Utils.FormatCount(closestTargetData![unitXPosition], closestSeries.UnitLabel ?? YAxisUnitLabel));
                var tipsize = Text.CalcSize(tip);
                bool up = false, left = false;
                if (tippos.x + tipsize.x > plot.width)
                {
                    left = true;
                    tippos.x -= tipsize.x + 2 * Spacing;
                }

                if (tippos.y + tipsize.y > plot.height)
                {
                    up = true;
                    tippos.y -= tipsize.y + 2 * Spacing;
                }

                var anchor = TextAnchor.UpperLeft;
                if (up && left)
                {
                    anchor = TextAnchor.LowerRight;
                }

                if (up && !left)
                {
                    anchor = TextAnchor.LowerLeft;
                }

                if (!up && left)
                {
                    anchor = TextAnchor.UpperRight;
                }

                var tooltipRect = new Rect(tippos.x, tippos.y, tipsize.x, tipsize.y);
                using (GUIScope.Multiple(gameFont: GameFont.Tiny, textAnchor: anchor))
                {
                    Widgets.Label(tooltipRect, tip);
                }
            }
        }
    }
}

public class GraphSeries
{
    public bool Hidden { get; set; }
    public string Label { get; set; } = string.Empty;
    public string UnitLabel { get; set; } = string.Empty;

    private Color _color = Color.white;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            _mutedColor = null;
        }
    }

    private Color? _mutedColor;
    public Color MutedColor
    {
        get
        {
            if (!_mutedColor.HasValue)
            {
                Color.RGBToHSV(Color, out var H, out var S, out var V);
                S /= 2;
                V /= 2;
                _mutedColor = Color.HSVToRGB(H, S, V);
            }
            return _mutedColor.Value;
        }
    }
}

[Serializable]
public class GraphException : Exception
{
    public GraphException() { }
    public GraphException(string message) : base(message) { }
    public GraphException(string message, Exception inner) : base(message, inner) { }
    protected GraphException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

#if DEBUG
[HotSwappable]
internal class GraphTest_Dialog : Window
{
    private GraphRenderer _emptyGraphRenderer = new([]);
    private GraphRenderer _singleGraphRenderer = new([
        new()
        {
            Label = "Series 1"
        }
    ]);
    private GraphRenderer _multipleGraphRenderer = new([
        new()
        {
            Label = "Series 1",
            Color = ColorLibrary.BrickRed
        },
        new()
        {
            Label = "Series 2",
            Color = ColorLibrary.Mustard
        },
        new()
        {
            Label = "Series 3",
            Color = ColorLibrary.GrassGreen
        }
    ])
    {
        DrawTargetLine = true,
    };
    private GraphRenderer _multipleWithHiddenGraphRenderer = new([
        new()
        {
            Label = "Series 1",
            Color = ColorLibrary.BrickRed,
            Hidden = true,
        },
        new()
        {
            Label = "Series 2",
            Color = ColorLibrary.Mustard
        },
        new()
        {
            Label = "Series 3",
            Color = ColorLibrary.GrassGreen
        }
    ])
    {
        DrawTargetLine = true,
    };

    public override Vector2 InitialSize => new(1024, 768);

    private int _currentTab;
    public override void DoWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard
        {
            ColumnWidth = (int)(inRect.width / 3.2)
        };

        listing.Begin(inRect);

        DoSetting(
            g => g.DrawInlineLegend,
            (g, v) => g.DrawInlineLegend = v,
            (ref bool r) => listing.CheckboxLabeled("Draw inline legend", ref r));

        DoSetting(
            g => g.LegendLineLength,
            (g, v) => g.LegendLineLength = v,
            (ref float r) => r = listing.SliderLabeled("Legend line length", r, 1, inRect.width / 3));

        listing.NewColumn();

        DoSetting(
            g => g.DrawTargetLine,
            (g, v) => g.DrawTargetLine = v,
            (ref bool r) => listing.CheckboxLabeled("Draw target line", ref r));

        listing.NewColumn();

        DoSetting(
            g => g.Interactive,
            (g, v) => g.Interactive = v,
            (ref bool r) => listing.CheckboxLabeled("Interactive", ref r));

        listing.End();

        inRect.yMin += 32f + listing.MaxColumnHeightSeen;

        var tabs = new List<TabRecord>() {
            new("Empty", () =>
            {
                _currentTab = 0;
            }, _currentTab == 0),
            new("Single", () =>
            {
                _currentTab = 1;
            }, _currentTab == 1),
            new("Multiple", () =>
            {
                _currentTab = 2;
            }, _currentTab == 2),
            new("Multiple (w/Hidden)", () =>
            {
                _currentTab = 3;
            }, _currentTab == 3)
        };
        Widgets.DrawMenuSection(inRect);
        TabDrawer.DrawTabs(inRect, tabs);
        switch (_currentTab)
        {
            case 0:
                _emptyGraphRenderer.DrawGraph(inRect, [], []);
                break;

            case 1:
                _singleGraphRenderer.DrawGraph(inRect, [[2, 4, 6, 8]], [[5, 5, 5, 5]]);
                break;

            case 2:
                _multipleGraphRenderer.DrawGraph(inRect, [
                    [2, 4, 6, 8],
                    [1, 2, 3, 4],
                    [5, 15, 2, 7]
                ], [
                    [10, 10, 10, 10],
                    null,
                    [8, 20, 8, 8]
                ]);
                break;

            case 3:
                _multipleWithHiddenGraphRenderer.DrawGraph(inRect, [
                    [1, 2, 3, 4],
                    [5, 15, 2, 7]
                ], [
                    null,
                    [8, 20, 8, 8]
                ]);
                break;
        }
    }

    private delegate void ModifyValue<T>(ref T value);
    private void DoSetting<T>(
        Func<GraphRenderer, T> getValue,
        Action<GraphRenderer, T> setValue,
        ModifyValue<T> modifyValue)
    {
        var value = getValue(_emptyGraphRenderer);
        modifyValue(ref value);
        setValue(_emptyGraphRenderer, value);
        setValue(_singleGraphRenderer, value);
        setValue(_multipleGraphRenderer, value);
        setValue(_multipleWithHiddenGraphRenderer, value);
    }
}

internal static class GraphTestDebugAction
{
    [IlyvionDebugAction(
        "ilyvion's Laboratory",
        "Show graph test dialog",
        allowedGameStates = AllowedGameStates.Playing)]
    private static void ShowGraphTestDialog()
    {
        Find.WindowStack.Add(new GraphTest_Dialog());
    }

}
#endif
