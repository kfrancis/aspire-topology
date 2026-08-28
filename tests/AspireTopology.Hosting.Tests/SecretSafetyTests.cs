using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Extraction;
using AspireTopology.Model;
using AspireTopology.Serialization;

namespace AspireTopology.Hosting.Tests;

/// <summary>
/// AspireTopology never serializes secret values, environment values, parameter values,
/// credentials or connection strings. Endpoints are fine; anything that could carry a secret is
/// not. These tests exist so that rule stays true as the extractor grows.
/// </summary>
public class SecretSafetyTests
{
    private const string SecretValue = "sup3r-s3cret-value";

    private static DistributedApplicationModel ModelWithSecrets()
    {
        var password = new ParameterResource("db-password", _ => SecretValue, secret: true);

        var postgres = new PostgresResource("postgres");
        postgres.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
            context.EnvironmentVariables["POSTGRES_PASSWORD"] = SecretValue));

        var api = new ProjectResource("api").WithReferenceTo(postgres);

        return new DistributedApplicationModel([password, postgres, api]);
    }

    [Test]
    public async Task Extract_DoesNotEmitEnvironmentOrParameterValues()
    {
        var topology = new AspireTopologyExtractor(new TopologyDiagramOptions { IncludeParameters = true })
            .Extract(ModelWithSecrets());

        var json = TopologyJson.Serialize(topology);

        await Assert.That(json.Contains(SecretValue, StringComparison.Ordinal)).IsFalse();
        await Assert.That(json.Contains("POSTGRES_PASSWORD", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task Extract_ExcludesParametersByDefault()
    {
        var topology = new AspireTopologyExtractor().Extract(ModelWithSecrets());

        await Assert.That(topology.FindNode("db-password")).IsNull();
    }

    [Test]
    public async Task Extract_IncludesParametersWhenAskedButNotTheirValues()
    {
        var topology = new AspireTopologyExtractor(new TopologyDiagramOptions { IncludeParameters = true })
            .Extract(ModelWithSecrets());

        var node = topology.FindNode("db-password")!;

        await Assert.That(node.Kind).IsEqualTo(TopologyNodeKind.Parameter);
        await Assert.That(node.Properties.Values.Any(value => value is string text && text.Contains(SecretValue, StringComparison.Ordinal))).IsFalse();
    }
}
