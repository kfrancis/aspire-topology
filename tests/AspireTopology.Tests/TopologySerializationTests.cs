using AspireTopology.Layout;
using AspireTopology.Model;
using AspireTopology.Serialization;

namespace AspireTopology.Tests;

public class TopologySerializationTests
{
    private static TopologyDocument Sample() => new(
        "sample",
        [
            new TopologyNode("api", "Public API", TopologyNodeKind.Service, new Dictionary<string, object?>
            {
                ["aspire.resourceType"] = "ProjectResource",
                ["endpoint.http.port"] = 8080,
                ["endpoint.http.external"] = true,
            }),
            new TopologyNode("appdb", "appdb", TopologyNodeKind.Database),
        ],
        [new TopologyEdge("api--reference--appdb", "api", "appdb", TopologyEdgeKind.Reference)],
        [new TopologyGroup("group-Backend", "Backend", TopologyGroupKind.Logical, ["api"])]);

    [Test]
    public async Task Serialize_UsesCamelCaseNamesAndStringEnums()
    {
        var json = TopologyJson.Serialize(Sample());

        await Assert.That(json.Contains("\"nodes\"", StringComparison.Ordinal)).IsTrue();
        await Assert.That(json.Contains("\"kind\": \"service\"", StringComparison.Ordinal)).IsTrue();
        await Assert.That(json.Contains("\"kind\": \"reference\"", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task Serialize_PreservesPropertyKeyCasing()
    {
        var json = TopologyJson.Serialize(Sample());

        // Property bag keys are data, not CLR member names, so they must survive verbatim.
        await Assert.That(json.Contains("\"aspire.resourceType\"", StringComparison.Ordinal)).IsTrue();
        await Assert.That(json.Contains("\"endpoint.http.port\"", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task RoundTrip_PreservesTopology()
    {
        var original = Sample();

        var restored = TopologyJson.Deserialize(TopologyJson.Serialize(original));

        await Assert.That(restored.Name).IsEqualTo(original.Name);
        await Assert.That(restored.Nodes.Count).IsEqualTo(2);
        await Assert.That(restored.Nodes[0].Kind).IsEqualTo(TopologyNodeKind.Service);
        await Assert.That(restored.Edges[0].Kind).IsEqualTo(TopologyEdgeKind.Reference);
        await Assert.That(restored.Groups[0].Kind).IsEqualTo(TopologyGroupKind.Logical);
        await Assert.That(restored.Groups[0].NodeIds[0]).IsEqualTo("api");
    }

    [Test]
    public async Task Serialize_IsStableAcrossCalls()
    {
        // Generated artifacts land in source control, so the same input must produce the same bytes.
        await Assert.That(TopologyJson.Serialize(Sample())).IsEqualTo(TopologyJson.Serialize(Sample()));
    }

    [Test]
    public async Task Layout_RoundTrips()
    {
        var layout = new TopologyLayout(new Dictionary<string, TopologyPosition>
        {
            ["api"] = new TopologyPosition(1, 2),
        });

        var restored = TopologyJson.DeserializeLayout(TopologyJson.SerializeLayout(layout));

        await Assert.That(restored.Find("api")!.X).IsEqualTo(1d);
        await Assert.That(restored.Find("api")!.Y).IsEqualTo(2d);
    }
}
