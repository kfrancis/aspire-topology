using AspireTopology.Layout;
using AspireTopology.Model;

namespace AspireTopology.Tests;

public class LayeredTopologyLayoutEngineTests
{
    private static TopologyDocument WebApiDatabase() => new(
        "sample",
        [
            new TopologyNode("web", "web", TopologyNodeKind.Service),
            new TopologyNode("api", "api", TopologyNodeKind.Service),
            new TopologyNode("appdb", "appdb", TopologyNodeKind.Database),
            new TopologyNode("cache", "cache", TopologyNodeKind.Cache),
        ],
        [
            new TopologyEdge("e1", "web", "api", TopologyEdgeKind.Reference),
            new TopologyEdge("e2", "api", "appdb", TopologyEdgeKind.Reference),
            new TopologyEdge("e3", "api", "cache", TopologyEdgeKind.Reference),
        ]);

    [Test]
    public async Task Layout_PlacesDependenciesBelowTheirDependents()
    {
        var layout = new LayeredTopologyLayoutEngine().Layout(WebApiDatabase());

        await Assert.That(layout.Find("web")!.Y).IsEqualTo(0d);
        await Assert.That(layout.Find("api")!.Y).IsEqualTo(1d);
        await Assert.That(layout.Find("appdb")!.Y).IsEqualTo(2d);
        await Assert.That(layout.Find("cache")!.Y).IsEqualTo(2d);
    }

    [Test]
    public async Task Layout_SpreadsNodesInTheSameLayer()
    {
        var layout = new LayeredTopologyLayoutEngine().Layout(WebApiDatabase());

        await Assert.That(layout.Find("appdb")!.X).IsNotEqualTo(layout.Find("cache")!.X);
    }

    [Test]
    public async Task Layout_IgnoresContainmentWhenComputingDepth()
    {
        var topology = new TopologyDocument(
            "sample",
            [
                new TopologyNode("postgres", "postgres", TopologyNodeKind.Database),
                new TopologyNode("appdb", "appdb", TopologyNodeKind.Database),
            ],
            [new TopologyEdge("e1", "appdb", "postgres", TopologyEdgeKind.Parent)]);

        var layout = new LayeredTopologyLayoutEngine().Layout(topology);

        await Assert.That(layout.Find("appdb")!.Y).IsEqualTo(0d);
        await Assert.That(layout.Find("postgres")!.Y).IsEqualTo(0d);
    }

    [Test]
    public async Task Layout_TerminatesOnCycles()
    {
        var topology = new TopologyDocument(
            "sample",
            [
                new TopologyNode("a", "a", TopologyNodeKind.Service),
                new TopologyNode("b", "b", TopologyNodeKind.Service),
            ],
            [
                new TopologyEdge("e1", "a", "b", TopologyEdgeKind.Reference),
                new TopologyEdge("e2", "b", "a", TopologyEdgeKind.Reference),
            ]);

        var layout = new LayeredTopologyLayoutEngine().Layout(topology);

        await Assert.That(layout.Nodes.Count).IsEqualTo(2);
    }

    [Test]
    public async Task WithOverrides_LetsSavedPositionsWin()
    {
        var generated = new LayeredTopologyLayoutEngine().Layout(WebApiDatabase());
        var human = new TopologyLayout(new Dictionary<string, TopologyPosition>
        {
            ["api"] = new TopologyPosition(42, 43),
        });

        var merged = generated.WithOverrides(human);

        await Assert.That(merged.Find("api")!.X).IsEqualTo(42d);
        await Assert.That(merged.Find("web")!.Y).IsEqualTo(0d);
    }
}
