using AspireTopology.Model;

namespace AspireTopology.Tests;

public class TopologyModelTests
{
    [Test]
    public async Task Empty_HasNoNodesEdgesOrGroups()
    {
        var document = TopologyDocument.Empty("app");

        await Assert.That(document.Name).IsEqualTo("app");
        await Assert.That(document.Nodes.Count).IsEqualTo(0);
        await Assert.That(document.Edges.Count).IsEqualTo(0);
        await Assert.That(document.Groups.Count).IsEqualTo(0);
    }

    [Test]
    public async Task FindNode_ReturnsMatchingNode()
    {
        var document = new TopologyDocument(
            "app",
            [new TopologyNode("api", "Public API", TopologyNodeKind.Service)],
            []);

        await Assert.That(document.FindNode("api")!.Name).IsEqualTo("Public API");
        await Assert.That(document.FindNode("missing")).IsNull();
    }

    [Test]
    public async Task Node_WithoutProperties_UsesSharedEmptyBag()
    {
        var node = new TopologyNode("api", "api", TopologyNodeKind.Service);

        await Assert.That(node.Properties.Count).IsEqualTo(0);
    }
}
