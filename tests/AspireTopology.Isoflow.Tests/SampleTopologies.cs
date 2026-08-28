using AspireTopology.Model;

namespace AspireTopology.Isoflow.Tests;

internal static class SampleTopologies
{
    /// <summary>A web front end talking to an API.</summary>
    public static TopologyDocument SimpleApp() => new(
        "simple-app",
        [
            new TopologyNode("api", "api", TopologyNodeKind.Service),
            new TopologyNode("web", "web", TopologyNodeKind.Service),
        ],
        [new TopologyEdge("web--reference--api", "web", "api", TopologyEdgeKind.Reference)]);

    /// <summary>The shape the sample AppHost produces.</summary>
    public static TopologyDocument PostgresAndRedis() => new(
        "postgres-redis",
        [
            new TopologyNode("api", "api", TopologyNodeKind.Service, new Dictionary<string, object?>
            {
                ["aspire.resourceType"] = "ProjectResource",
                ["endpoint.http.targetPort"] = 8080,
            }),
            new TopologyNode("appdb", "appdb", TopologyNodeKind.Database),
            new TopologyNode("cache", "cache", TopologyNodeKind.Cache, new Dictionary<string, object?>
            {
                ["container.image"] = "redis",
                ["container.tag"] = "7.4",
            }),
            new TopologyNode("postgres", "postgres", TopologyNodeKind.Database),
            new TopologyNode("web", "web", TopologyNodeKind.Service),
        ],
        [
            new TopologyEdge("api--reference--appdb", "api", "appdb", TopologyEdgeKind.Reference),
            new TopologyEdge("api--dependency--appdb", "api", "appdb", TopologyEdgeKind.Dependency),
            new TopologyEdge("api--reference--cache", "api", "cache", TopologyEdgeKind.Reference),
            new TopologyEdge("appdb--parent--postgres", "appdb", "postgres", TopologyEdgeKind.Parent),
            new TopologyEdge("web--reference--api", "web", "api", TopologyEdgeKind.Reference),
        ],
        [new TopologyGroup("contains-postgres", "postgres", TopologyGroupKind.Containment, ["appdb", "postgres"])]);
}
