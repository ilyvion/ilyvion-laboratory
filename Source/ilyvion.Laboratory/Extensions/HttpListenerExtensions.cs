using System.Net;

namespace ilyvion.Laboratory.Extensions;

public static class HttpListenerExtensions
{
    public static void WriteBytes(this HttpListenerResponse response, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(data);

        response.ContentLength64 = data.LongLength;
        response.OutputStream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Writes <paramref name="data"/> as the response body, with a content type picked from the
    /// extension of <paramref name="path"/>.
    /// </summary>
    public static void WriteFile(this HttpListenerResponse response, string path, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.ContentType = ContentTypeForPath(path);
        response.WriteBytes(data);
    }

    /// <summary>
    /// The URL's path split into its non-empty segments, e.g. <c>["api", "items"]</c> for
    /// <c>/api/items/</c>.
    /// </summary>
    public static string[] PathParts(this Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);

        return url.AbsolutePath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
    }

    internal static string ContentTypeForPath(string path) =>
        Path.GetExtension(path).ToUpperInvariant() switch
        {
            ".HTML" => "text/html",
            ".CSS" => "text/css",
            ".JS" => "application/javascript",
            ".JSON" => "application/json",
            ".SVG" => "image/svg+xml",
            ".PNG" => "image/png",
            ".JPG" or ".JPEG" => "image/jpeg",
            ".GIF" => "image/gif",
            ".ICO" => "image/x-icon",
            ".TXT" => "text/plain",
            _ => "application/octet-stream",
        };
}
