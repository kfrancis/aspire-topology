namespace AspireTopology.Hosting.Tests;

/// <summary>
/// The defaults are the whole user experience of <c>AddTopologyDiagram()</c>: one call, and both
/// running the app and the dashboard entry work without further configuration. Changing any of
/// these changes what a consumer gets from a bare call, so they are pinned here.
/// </summary>
public class TopologyDiagramOptionsTests
{
    [Test]
    public async Task Defaults_RefreshArtifactsWhenTheAppHostStarts()
    {
        await Assert.That(new TopologyDiagramOptions().GenerateOnStart).IsTrue();
    }

    [Test]
    public async Task Defaults_ListTheViewerInTheDashboard()
    {
        await Assert.That(new TopologyDiagramOptions().Viewer).IsTrue();
    }

    [Test]
    public async Task Defaults_ExcludeParametersAndIncludeEndpoints()
    {
        var options = new TopologyDiagramOptions();

        // Parameters are configuration, not architecture, and their values must never be written.
        await Assert.That(options.IncludeParameters).IsFalse();
        await Assert.That(options.IncludeEndpoints).IsTrue();
    }

    [Test]
    public async Task Defaults_WriteIsoflowNextToTheTopologyDocument()
    {
        var options = new TopologyDiagramOptions();

        await Assert.That(options.OutputPath).IsEqualTo("artifacts/topology");
        await Assert.That(options.FileName).IsEqualTo("topology");
        await Assert.That(options.ViewerResourceName).IsEqualTo("topology");
        await Assert.That(options.Renderers.Count).IsEqualTo(1);
        await Assert.That(options.Renderers[0].Name).IsEqualTo("isoflow");
    }
}
