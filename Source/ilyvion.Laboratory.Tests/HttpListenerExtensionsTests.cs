// HttpListenerExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.Net;
using System.Net.Sockets;
using System.Text;
using DevTools.Testing;
using ilyvion.Laboratory.Extensions;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class HttpListenerExtensionsTests
{
    private static int FindFreeTcpPort()
    {
#if v1_7_OR_GREATER
        using
#endif
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        return port;
    }

    // Serves one request through a real HttpListener, answering it with respond, and returns the
    // raw HTTP response text. HttpListenerResponse has no public constructor to test against.
    private static string RoundTrip(string requestPath, Action<HttpListenerContext> respond)
    {
        var port = FindFreeTcpPort();
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        string? responseText = null;
        Exception? clientException = null;
        var client = new Thread(() =>
        {
            try
            {
                using var tcp = new TcpClient("localhost", port);
                using var stream = tcp.GetStream();
                var request = Encoding.ASCII.GetBytes(
                    $"GET {requestPath} HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n"
                );
                stream.Write(request, 0, request.Length);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                responseText = reader.ReadToEnd();
            }
            catch (Exception e)
            {
                clientException = e;
            }
        });
        client.Start();

        var pending = listener.BeginGetContext(null, null);
        if (!pending.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("No request reached the listener.");
        }
        var context = listener.EndGetContext(pending);
        respond(context);
        context.Response.Close();

        var clientFinished = client.Join(TimeSpan.FromSeconds(5));
        return !clientFinished
                ? throw new TimeoutException("The client never finished reading the response.")
            : clientException != null
                ? throw new InvalidOperationException("The client failed.", clientException)
            : responseText ?? "";
    }

    [Test]
    public static void PathPartsSplitsTheAbsolutePathIntoNonEmptySegments()
    {
        var parts = new Uri("http://localhost:8080//api/items/?query=a/b").PathParts();

        Expect.That(() => parts.SequenceEqual(["api", "items"]));
    }

    [Test]
    public static void PathPartsOfTheRootIsEmpty()
    {
        var parts = new Uri("http://localhost:8080/").PathParts();

        Expect.AreEqual(0, parts.Length);
    }

    [Test]
    public static void ContentTypeForPathIgnoresTheExtensionsCase()
    {
        Expect.AreEqual("text/html", HttpListenerExtensions.ContentTypeForPath("index.html"));
        Expect.AreEqual("image/jpeg", HttpListenerExtensions.ContentTypeForPath("a/B.JPEG"));
        Expect.AreEqual("image/jpeg", HttpListenerExtensions.ContentTypeForPath("photo.Jpg"));
    }

    [Test]
    public static void ContentTypeForPathFallsBackToOctetStream()
    {
        Expect.AreEqual(
            "application/octet-stream",
            HttpListenerExtensions.ContentTypeForPath("archive.zip")
        );
        Expect.AreEqual(
            "application/octet-stream",
            HttpListenerExtensions.ContentTypeForPath("no-extension")
        );
    }

    [Test]
    public static void WriteFileSendsTheBodyWithItsContentTypeAndLength()
    {
        const string body = "body { color: red; }";

        var response = RoundTrip(
            "/styles/site.css",
            context => context.Response.WriteFile("site.css", Encoding.UTF8.GetBytes(body))
        );

        Expect.IsTrue(
            response.Contains("Content-Type: text/css\r\n", StringComparison.Ordinal),
            $"Content-Type header missing from:\n{response}"
        );
        Expect.IsTrue(
            response.Contains($"Content-Length: {body.Length}\r\n", StringComparison.Ordinal),
            $"Content-Length header missing from:\n{response}"
        );
        Expect.IsTrue(response.EndsWith("\r\n\r\n" + body, StringComparison.Ordinal));
    }
}
