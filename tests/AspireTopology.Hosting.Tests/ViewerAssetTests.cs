using AspireTopology.Hosting.Viewer;

namespace AspireTopology.Hosting.Tests;

/// <summary>
/// The viewer front end is built by <c>viewer/AspireTopology.Viewer</c>, checked in under its
/// <c>dist</c> directory and embedded by an MSBuild glob. Nothing else would notice if that glob
/// stopped matching, so these tests do.
/// </summary>
public class ViewerAssetTests
{
    [Test]
    public async Task Assets_AreEmbedded()
    {
        await Assert.That(TopologyViewerAssets.Any).IsTrue();
    }

    [Test]
    public async Task Assets_ContainTheEntryDocument()
    {
        using var index = TopologyViewerAssets.Open(TopologyViewerAssets.IndexPath);

        await Assert.That(index).IsNotNull();

        using var reader = new StreamReader(index!);
        var html = await reader.ReadToEndAsync();

        await Assert.That(html.Contains("<div id=\"root\">", StringComparison.Ordinal)).IsTrue();
        await Assert.That(html.Contains("<script", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task Assets_AreReachableUnderNestedPaths()
    {
        // Vite emits the bundle under assets/, so the logical names must keep the directory.
        using var index = TopologyViewerAssets.Open(TopologyViewerAssets.IndexPath)!;
        using var reader = new StreamReader(index);
        var html = await reader.ReadToEndAsync();

        var start = html.IndexOf("assets/", StringComparison.Ordinal);
        await Assert.That(start).IsGreaterThan(-1);

        var end = html.IndexOf('"', start);
        var bundlePath = html[start..end];

        using var bundle = TopologyViewerAssets.Open(bundlePath);
        await Assert.That(bundle).IsNotNull();
    }

    [Test]
    [Arguments("index.html", "text/html; charset=utf-8")]
    [Arguments("assets/index-abc.js", "text/javascript; charset=utf-8")]
    [Arguments("assets/style.css", "text/css; charset=utf-8")]
    [Arguments("icon.svg", "image/svg+xml")]
    [Arguments("something.unknown", "application/octet-stream")]
    public async Task ContentTypeFor_MapsExtensions(string path, string expected)
    {
        await Assert.That(TopologyViewerAssets.ContentTypeFor(path)).IsEqualTo(expected);
    }
}
