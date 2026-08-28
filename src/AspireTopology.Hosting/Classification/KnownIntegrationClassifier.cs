using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting.ApplicationModel;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Classification;

/// <summary>
/// Classifies the resources shipped by the common Aspire integrations by matching their type
/// names, rather than by referencing every integration package.
/// </summary>
/// <remarks>
/// Matching on type names keeps AspireTopology.Hosting dependent on Aspire.Hosting alone. It also
/// means a new integration that follows the usual naming, for example
/// <c>SomeVendorPostgresResource</c>, is classified correctly without a code change.
/// </remarks>
public sealed class KnownIntegrationClassifier : ITopologyResourceClassifier
{
    // Order matters. The first matching group wins, so a storage queue resource is storage rather
    // than a message broker.
    private static readonly (TopologyNodeKind Kind, string[] Tokens)[] Rules =
    [
        (TopologyNodeKind.Cache, ["Redis", "Valkey", "Garnet", "Memcached"]),
        (TopologyNodeKind.Storage, ["Storage", "Blob", "S3", "FileShare", "DataLake"]),
        (TopologyNodeKind.MessageBroker, ["RabbitMQ", "Kafka", "ServiceBus", "EventHub", "Nats", "Pulsar", "EventGrid"]),
        (TopologyNodeKind.Database, ["Postgres", "SqlServer", "MySql", "MariaDb", "Oracle", "MongoDB", "Sqlite", "Cosmos", "Cassandra", "Elasticsearch", "OpenSearch", "Milvus", "Qdrant", "SurrealDb", "Database"]),
        (TopologyNodeKind.ExternalService, ["ExternalService"]),
    ];

    /// <inheritdoc />
    public bool TryClassify(IResource resource, [NotNullWhen(true)] out TopologyNodeDescriptor? descriptor)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var typeName = resource.GetType().Name;

        foreach (var (kind, tokens) in Rules)
        {
            if (tokens.Any(token => typeName.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                descriptor = new TopologyNodeDescriptor(kind);
                return true;
            }
        }

        descriptor = null;
        return false;
    }
}
