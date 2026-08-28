using AspireTopology.Isoflow.Mapping;
using AspireTopology.Model;

namespace AspireTopology.Isoflow.Tests;

public class TopologyToIsoflowMapperTests
{
    [Test]
    public async Task Map_CreatesOneItemPerNode()
    {
        var document = new TopologyToIsoflowMapper().Map(SampleTopologies.PostgresAndRedis());

        await Assert.That(document.Items.Count).IsEqualTo(5);
        await Assert.That(document.Title).IsEqualTo("postgres-redis");
    }

    [Test]
    public async Task Map_PlacesEveryItemInTheView()
    {
        var document = new TopologyToIsoflowMapper().Map(SampleTopologies.PostgresAndRedis());
        var view = document.Views.Single();

        await Assert.That(view.Items.Count).IsEqualTo(document.Items.Count);
    }

    [Test]
    public async Task Map_GivesEveryItemADistinctTile()
    {
        var view = new TopologyToIsoflowMapper().Map(SampleTopologies.PostgresAndRedis()).Views.Single();

        var distinct = view.Items.Select(item => (item.Tile.X, item.Tile.Y)).Distinct().Count();

        await Assert.That(distinct).IsEqualTo(view.Items.Count);
    }

    [Test]
    public async Task Map_EmitsOneIconPerNodeKindUsed()
    {
        var document = new TopologyToIsoflowMapper().Map(SampleTopologies.PostgresAndRedis());

        // Service, Database and Cache.
        await Assert.That(document.Icons.Count).IsEqualTo(3);
        await Assert.That(document.Icons.All(icon => icon.Url.StartsWith("data:image/svg+xml;base64,", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task Map_ReferencesOnlyIconsItDeclares()
    {
        var document = new TopologyToIsoflowMapper().Map(SampleTopologies.PostgresAndRedis());
        var declared = document.Icons.Select(icon => icon.Id).ToHashSet(StringComparer.Ordinal);

        await Assert.That(document.Items.All(item => declared.Contains(item.Icon))).IsTrue();
    }

    [Test]
    public async Task Map_ConnectsEveryEdgeBetweenPlacedItems()
    {
        var topology = SampleTopologies.PostgresAndRedis();
        var view = new TopologyToIsoflowMapper().Map(topology).Views.Single();

        await Assert.That(view.Connectors.Count).IsEqualTo(topology.Edges.Count);
        await Assert.That(view.Connectors.All(connector => connector.Anchors.Count == 2)).IsTrue();
    }

    [Test]
    public async Task Map_DrawsDependenciesDashed()
    {
        var view = new TopologyToIsoflowMapper().Map(SampleTopologies.PostgresAndRedis()).Views.Single();

        var dependency = view.Connectors.Single(connector => connector.Description == nameof(TopologyEdgeKind.Dependency));

        await Assert.That(dependency.Style).IsEqualTo("DASHED");
    }

    [Test]
    public async Task Map_DrawsGroupsAsRectangles()
    {
        var view = new TopologyToIsoflowMapper().Map(SampleTopologies.PostgresAndRedis()).Views.Single();

        await Assert.That(view.Rectangles.Count).IsEqualTo(1);
        await Assert.That(view.Rectangles[0].Id).IsEqualTo("rect-contains-postgres");
    }

    [Test]
    public async Task Map_SkipsGroupRectanglesWhenDisabled()
    {
        var options = new IsoflowRendererOptions { RenderGroupRectangles = false };

        var view = new TopologyToIsoflowMapper(options).Map(SampleTopologies.PostgresAndRedis()).Views.Single();

        await Assert.That(view.Rectangles.Count).IsEqualTo(0);
    }

    [Test]
    public async Task RenderAsync_WritesTheSameJsonAsRenderToString()
    {
        var topology = SampleTopologies.SimpleApp();
        var renderer = new IsoflowTopologyRenderer();

        using var stream = new MemoryStream();
        await renderer.RenderAsync(topology, stream);

        var streamed = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        await Assert.That(streamed).IsEqualTo(renderer.RenderToString(topology));
    }
}
