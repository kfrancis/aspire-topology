using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Extraction;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Tests;

public class ExtractionTests
{
    private static TopologyDocument Extract(DistributedApplicationModel model, TopologyDiagramOptions? options = null) =>
        new AspireTopologyExtractor(options ?? new TopologyDiagramOptions { DocumentName = "sample" }).Extract(model);

    private static bool HasNode(TopologyDocument topology, string id, TopologyNodeKind kind) =>
        topology.Nodes.Any(node => node.Id == id && node.Kind == kind);

    private static bool HasEdge(TopologyDocument topology, string source, string target, TopologyEdgeKind kind) =>
        topology.Edges.Any(edge => edge.SourceId == source && edge.TargetId == target && edge.Kind == kind);

    [Test]
    public async Task Extract_ClassifiesResourcesSemantically()
    {
        var topology = Extract(SampleApplication.Build());

        await Assert.That(HasNode(topology, "api", TopologyNodeKind.Service)).IsTrue();
        await Assert.That(HasNode(topology, "web", TopologyNodeKind.Service)).IsTrue();
        await Assert.That(HasNode(topology, "postgres", TopologyNodeKind.Database)).IsTrue();
        await Assert.That(HasNode(topology, "appdb", TopologyNodeKind.Database)).IsTrue();
        await Assert.That(HasNode(topology, "cache", TopologyNodeKind.Cache)).IsTrue();
    }

    [Test]
    public async Task Extract_DiscoversReferences()
    {
        var topology = Extract(SampleApplication.Build());

        await Assert.That(HasEdge(topology, "api", "appdb", TopologyEdgeKind.Reference)).IsTrue();
        await Assert.That(HasEdge(topology, "api", "cache", TopologyEdgeKind.Reference)).IsTrue();
        await Assert.That(HasEdge(topology, "web", "api", TopologyEdgeKind.Reference)).IsTrue();
    }

    [Test]
    public async Task Extract_DistinguishesWaitForFromReference()
    {
        var topology = Extract(SampleApplication.Build());

        await Assert.That(HasEdge(topology, "api", "appdb", TopologyEdgeKind.Dependency)).IsTrue();
        await Assert.That(HasEdge(topology, "api", "cache", TopologyEdgeKind.Dependency)).IsFalse();
    }

    [Test]
    public async Task Extract_DiscoversContainment()
    {
        var topology = Extract(SampleApplication.Build());

        await Assert.That(HasEdge(topology, "appdb", "postgres", TopologyEdgeKind.Parent)).IsTrue();
    }

    [Test]
    public async Task Extract_DoesNotDuplicateEdges()
    {
        // The sample declares containment twice: through IResourceWithParent and through a Parent
        // relationship annotation. That must still be one edge.
        var topology = Extract(SampleApplication.Build());

        var parentEdges = topology.Edges.Count(edge => edge.SourceId == "appdb" && edge.Kind == TopologyEdgeKind.Parent);

        await Assert.That(parentEdges).IsEqualTo(1);
    }

    [Test]
    public async Task Extract_BuildsContainmentGroup()
    {
        var topology = Extract(SampleApplication.Build());

        var group = topology.Groups.Single(g => g.Kind == TopologyGroupKind.Containment);

        await Assert.That(group.Id).IsEqualTo("contains-postgres");
        await Assert.That(group.NodeIds.Contains("appdb")).IsTrue();
        await Assert.That(group.NodeIds.Contains("postgres")).IsTrue();
    }

    [Test]
    public async Task Extract_KeepsUnknownResources()
    {
        var model = new DistributedApplicationModel([new SomeFutureResource("whatever")]);

        var topology = Extract(model);
        var node = topology.Nodes.Single();

        await Assert.That(node.Id).IsEqualTo("whatever");
        await Assert.That(node.Kind).IsEqualTo(TopologyNodeKind.Unknown);
        await Assert.That(node.Properties[TopologyPropertyNames.AspireResourceType]).IsEqualTo("SomeFutureResource");
    }

    [Test]
    public async Task Extract_RecordsEndpoints()
    {
        var topology = Extract(SampleApplication.Build());
        var api = topology.FindNode("api")!;

        await Assert.That(api.Properties[TopologyPropertyNames.Endpoint("http", "scheme")]).IsEqualTo("http");
        await Assert.That(api.Properties[TopologyPropertyNames.Endpoint("http", "targetPort")]).IsEqualTo(8080);
        await Assert.That(topology.FindNode("web")!.Properties[TopologyPropertyNames.Endpoint("https", "external")]).IsEquivalentTo(true);
    }

    [Test]
    public async Task Extract_OmitsEndpointsWhenDisabled()
    {
        var topology = Extract(SampleApplication.Build(), new TopologyDiagramOptions { IncludeEndpoints = false });

        var api = topology.FindNode("api")!;

        await Assert.That(api.Properties.Keys.Any(key => key.StartsWith("endpoint.", StringComparison.Ordinal))).IsFalse();
    }

    [Test]
    public async Task Extract_RecordsContainerImages()
    {
        var topology = Extract(SampleApplication.Build());
        var cache = topology.FindNode("cache")!;

        await Assert.That(cache.Properties[TopologyPropertyNames.ContainerImage]).IsEqualTo("redis");
        await Assert.That(cache.Properties[TopologyPropertyNames.ContainerTag]).IsEqualTo("7.4");
    }

    [Test]
    public async Task Extract_IsDeterministic()
    {
        var first = Extract(SampleApplication.Build());
        var second = Extract(SampleApplication.Build());

        await Assert.That(string.Join(",", first.Nodes.Select(n => n.Id)))
            .IsEqualTo(string.Join(",", second.Nodes.Select(n => n.Id)));
        await Assert.That(string.Join(",", first.Edges.Select(e => e.Id)))
            .IsEqualTo(string.Join(",", second.Edges.Select(e => e.Id)));
    }
}
