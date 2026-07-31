namespace ilyvion.Laboratory.Extensions;

public static class ColorExtensions
{
    public static Color Muted(this Color color, float factor = 2f)
    {
        Color.RGBToHSV(color, out var H, out var S, out var V);
        S /= factor;
        V /= factor;
        return Color.HSVToRGB(H, S, V);
    }
}
