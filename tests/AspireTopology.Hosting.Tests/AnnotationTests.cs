using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Annotations;
using AspireTopology.Hosting.Extraction;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Tests;

public class AnnotationTests
{
    private static TopologyDocument Extract(params IResource[] resources) =>
        new AspireTopologyExtractor(new TopologyDiagramOptions { DocumentName = "sample" })
            .Extract(new DistributedApplicationModel(resources));

    private static T WithMetadata<T>(T resource, Action<TopologyMetadataAnnotation> configure)
        where T : IResource
    {
        var annotation = new TopologyMetadataAnnotation();
        configure(annotation);
        resource.Annotations.Add(annotation);
        return resource;
    }

    [Test]
    public async Task DisplayName_OverridesResourceName()
    {
        var api = WithMetadata(new ProjectResource("api"), x => x.DisplayName = "Public API");

        var node = Extract(api).FindNode("api")!;

        await Assert.That(node.Id).IsEqualTo("api");
        await Assert.That(node.Name).IsEqualTo("Public API");
    }

    [Test]
    public async Task Kind_OverridesClassification()
    {
        var resource = WithMetadata(new SomeFutureResource("thing"), x => x.Kind = TopologyNodeKind.MessageBroker);

        await Assert.That(Extract(resource).FindNode("thing")!.Kind).IsEqualTo(TopologyNodeKind.MessageBroker);
    }

    [Test]
    public async Task Group_BecomesALogicalGroup()
    {
        var api = WithMetadata(new ProjectResource("api"), x => x.Group = "Backend");
        var worker = WithMetadata(new ProjectResource("worker"), x => x.Group = "Backend");

        var group = Extract(api, worker).Groups.Single(g => g.Kind == TopologyGroupKind.Logical);

        await Assert.That(group.Name).IsEqualTo("Backend");
        await Assert.That(group.NodeIds.Count).IsEqualTo(2);
    }

    [Test]
    public async Task HiddenResources_AreLeftOut()
    {
        // Run mode adds orchestration plumbing, such as the project rebuilders behind hot reload.
        // Aspire hides those from its own dashboard, and they are not architecture either.
        var rebuilder = new SomeFutureResource("api-rebuilder");
        rebuilder.Annotations.Add(new HiddenAnnotation(HiddenBehavior.Always));

        var topology = Extract(new ProjectResource("api"), rebuilder);

        await Assert.That(topology.Nodes.Count).IsEqualTo(1);
        await Assert.That(topology.FindNode("api-rebuilder")).IsNull();
    }

    [Test]
    public async Task ResourcesHiddenOnlyOnCompletion_AreKept()
    {
        // A migration job that vanishes from the dashboard once it succeeds is still architecture.
        var migrations = new SomeFutureResource("migrations");
        migrations.Annotations.Add(new HiddenAnnotation(HiddenBehavior.OnCompletion));

        await Assert.That(Extract(migrations).FindNode("migrations")).IsNotNull();
    }

    [Test]
    public async Task Exclude_RemovesTheResourceAndItsEdges()
    {
        var cache = new RedisResource("cache");
        var api = new ProjectResource("api").WithReferenceTo(cache);
        WithMetadata(cache, x => x.Exclude = true);

        var topology = Extract(api, cache);

        await Assert.That(topology.Nodes.Count).IsEqualTo(1);
        await Assert.That(topology.Edges.Count).IsEqualTo(0);
    }
}
